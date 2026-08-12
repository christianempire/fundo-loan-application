using System.Text.Json;
using Fundo.Loans.Application.IntegrationEvents;
using Fundo.Loans.Infrastructure.ExternalService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fundo.Loans.Infrastructure.Persistence.Outbox;

/// <summary>
/// Drains the outbox and delivers each message to the external service.
/// </summary>
/// <remarks>
/// This runs on its own, outside the request that answered the form, which is the
/// point: the applicant is not kept waiting on a third party, and a third party being
/// down cannot fail an application that was already approved and written.
///
/// Delivery is at-least-once. The alternative — marking a message processed before
/// sending it — would be at-most-once and could silently drop an approved customer,
/// which is the worse failure. The receiving end is idempotent to make up for it.
/// </remarks>
internal sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<OutboxOptions> _options;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(
        IServiceScopeFactory scopeFactory,
        IOptions<OutboxOptions> options,
        ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.Value.PollingIntervalSeconds));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ProcessPendingAsync(stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                // A bad batch must not take the processor down with it.
                _logger.LogError(exception, "The outbox batch failed.");
            }
        }
    }

    private async Task ProcessPendingAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<LoansDbContext>();
        var client = scope.ServiceProvider.GetRequiredService<ICustomerSyncClient>();

        var options = _options.Value;
        var now = DateTime.UtcNow;

        var pending = await context.OutboxMessages
            .Where(message =>
                message.ProcessedAt == null
                && message.AttemptCount < options.MaxAttempts
                && (message.NextAttemptAt == null || message.NextAttemptAt <= now))
            .OrderBy(message => message.OccurredAt)
            .Take(options.BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in pending)
        {
            await DeliverAsync(message, client, cancellationToken);
        }

        if (pending.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task DeliverAsync(
        OutboxMessage message,
        ICustomerSyncClient client,
        CancellationToken cancellationToken)
    {
        try
        {
            var @event = JsonSerializer.Deserialize<CustomerSyncRequested>(
                message.Payload, OutboxSerialization.Options)
                ?? throw new InvalidOperationException("The outbox payload deserialized to null.");

            await client.SendAsync(@event, cancellationToken);
            message.MarkProcessed();

            _logger.LogInformation(
                "Delivered outbox message {MessageId} ({Operation}) for customer {CustomerId}.",
                message.Id, @event.Operation, @event.CustomerId);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            message.MarkFailed(exception.Message, RetryDelayFor(message.AttemptCount));

            var attemptsLeft = _options.Value.MaxAttempts - message.AttemptCount;
            if (attemptsLeft > 0)
            {
                _logger.LogWarning(
                    exception,
                    "Outbox message {MessageId} failed on attempt {Attempt}; {Remaining} attempts left.",
                    message.Id, message.AttemptCount, attemptsLeft);
            }
            else
            {
                // Left in the table, unprocessed and with its last error, rather than
                // deleted: it is a record of an approved customer the partner never got.
                _logger.LogError(
                    exception,
                    "Outbox message {MessageId} gave up after {Attempts} attempts and needs attention.",
                    message.Id, message.AttemptCount);
            }
        }
    }

    /// <summary>Exponential backoff: 5s, 10s, 20s, 40s with the default settings.</summary>
    private TimeSpan RetryDelayFor(int attemptCount) =>
        TimeSpan.FromSeconds(_options.Value.BaseRetryDelaySeconds * Math.Pow(2, attemptCount));
}
