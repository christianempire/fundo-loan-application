using Fundo.Loans.Application.IntegrationEvents;

namespace Fundo.Loans.Application.Abstractions;

/// <summary>
/// Hands an event over for delivery outside the current request.
/// </summary>
/// <remarks>
/// Publishing has to be part of the same unit of work as the writes it describes,
/// so an implementation is expected to enlist in the ambient transaction rather than
/// to reach the outside world here. See the outbox in the infrastructure layer.
/// </remarks>
public interface IIntegrationEventPublisher
{
    Task PublishAsync(CustomerSyncRequested @event, CancellationToken cancellationToken);
}
