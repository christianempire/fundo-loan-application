using System.Text.Json;
using Fundo.Loans.Application.Abstractions;
using Fundo.Loans.Application.IntegrationEvents;

namespace Fundo.Loans.Infrastructure.Persistence.Outbox;

/// <summary>
/// Publishes by inserting a row, not by calling anyone.
/// </summary>
/// <remarks>
/// This is what makes the use case atomic. The insert joins the same change tracker
/// and therefore the same transaction as the customer and the application, so the
/// three commit or roll back together. Reaching the external service here would put
/// an HTTP call inside a database transaction and make "publish" impossible to undo.
/// </remarks>
internal sealed class OutboxEventPublisher : IIntegrationEventPublisher
{
    private readonly LoansDbContext _context;

    public OutboxEventPublisher(LoansDbContext context) => _context = context;

    public Task PublishAsync(CustomerSyncRequested @event, CancellationToken cancellationToken)
    {
        var message = OutboxMessage.For(
            type: nameof(CustomerSyncRequested),
            payload: JsonSerializer.Serialize(@event, OutboxSerialization.Options));

        _context.OutboxMessages.Add(message);

        // Saving is the unit of work's job; it owns the transaction boundary.
        return Task.CompletedTask;
    }
}

/// <summary>Shared so the processor reads back exactly what the publisher wrote.</summary>
internal static class OutboxSerialization
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}
