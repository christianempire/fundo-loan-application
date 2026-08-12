using System.Net.Http.Json;
using Fundo.Loans.Application.IntegrationEvents;

namespace Fundo.Loans.Infrastructure.ExternalService;

/// <summary>
/// Talks to the external service over HTTP: POST to create, PUT to update.
/// </summary>
/// <remarks>
/// The customer id we already own is the shared reference, so both verbs are
/// idempotent on it. That matters because the outbox guarantees at-least-once
/// delivery: the same message can legitimately arrive twice.
/// </remarks>
internal sealed class HttpCustomerSyncClient : ICustomerSyncClient
{
    private readonly HttpClient _httpClient;

    public HttpCustomerSyncClient(HttpClient httpClient) => _httpClient = httpClient;

    public async Task SendAsync(CustomerSyncRequested @event, CancellationToken cancellationToken)
    {
        var payload = new
        {
            @event.CustomerId,
            @event.FirstName,
            @event.LastName,
            @event.CompanyName,
            @event.Address,
            @event.SsnLast4,
            @event.ApplicationId,
            @event.RequestedAmount,
        };

        using var response = @event.Operation switch
        {
            CustomerSyncOperation.Create =>
                await _httpClient.PostAsJsonAsync("/customers", payload, cancellationToken),
            CustomerSyncOperation.Update =>
                await _httpClient.PutAsJsonAsync($"/customers/{@event.CustomerId}", payload, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(
                nameof(@event), @event.Operation, "Unknown sync operation."),
        };

        // Throwing is the contract: the outbox decides what a failure means.
        response.EnsureSuccessStatusCode();
    }
}
