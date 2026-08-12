using Fundo.Loans.Application.IntegrationEvents;

namespace Fundo.Loans.Infrastructure.ExternalService;

/// <summary>
/// Sends an approved application on to the external service. Implementations are
/// expected to throw when delivery fails, so the outbox can retry.
/// </summary>
public interface ICustomerSyncClient
{
    Task SendAsync(CustomerSyncRequested @event, CancellationToken cancellationToken);
}
