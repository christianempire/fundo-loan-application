using System.Net.Http.Json;
using System.Text.Json;
using Fundo.Loans.Application.IntegrationEvents;
using Microsoft.EntityFrameworkCore;

namespace Fundo.Loans.Tests.Integration;

/// <summary>
/// The same SSN applying twice must leave one customer and one application, updated,
/// and must tell the external service to update rather than create.
/// </summary>
public class ReturningCustomerTests
{
    private const string Endpoint = "/api/loan-applications";
    private const string Ssn = "444-55-6666";

    [Fact]
    public async Task Updates_the_customer_and_the_application_instead_of_duplicating_them()
    {
        using var factory = new LoansApiFactory();
        using var client = factory.CreateClient();

        var first = await client.PostAsJsonAsync(Endpoint, LoanApplicationRequests.Valid(
            ssn: Ssn, amount: 5_000m, companyName: "Analytical Engines"));

        var second = await client.PostAsJsonAsync(Endpoint, LoanApplicationRequests.Valid(
            ssn: Ssn, amount: 12_500m, companyName: "Difference Engines", lastName: "Byron"));

        var firstBody = await first.Content.ReadFromJsonAsync<JsonElement>();
        var secondBody = await second.Content.ReadFromJsonAsync<JsonElement>();

        // Same application, not a second one.
        Assert.Equal(
            firstBody.GetProperty("applicationId").GetGuid(),
            secondBody.GetProperty("applicationId").GetGuid());

        Assert.Equal(1, await factory.QueryAsync(context => context.Customers.CountAsync()));
        Assert.Equal(1, await factory.QueryAsync(context => context.LoanApplications.CountAsync()));

        var customer = await factory.QueryAsync(context => context.Customers.SingleAsync());
        var application = await factory.QueryAsync(context => context.LoanApplications.SingleAsync());

        Assert.Equal("Byron", customer.LastName);
        Assert.Equal("Difference Engines", customer.CompanyName);
        Assert.Equal(12_500m, application.RequestedAmount);
        Assert.Equal(customer.Id, application.CustomerId);
    }

    [Fact]
    public async Task Tells_the_external_service_to_create_once_and_then_to_update()
    {
        using var factory = new LoansApiFactory();
        using var client = factory.CreateClient();

        await client.PostAsJsonAsync(Endpoint, LoanApplicationRequests.Valid(ssn: Ssn, amount: 5_000m));
        await client.PostAsJsonAsync(Endpoint, LoanApplicationRequests.Valid(ssn: Ssn, amount: 12_500m));

        var messages = await factory.QueryAsync(context =>
            context.OutboxMessages.OrderBy(message => message.OccurredAt).ToListAsync());

        Assert.Equal(2, messages.Count);

        var events = messages
            .Select(message => JsonSerializer.Deserialize<CustomerSyncRequested>(
                message.Payload, new JsonSerializerOptions(JsonSerializerDefaults.Web))!)
            .ToList();

        Assert.Equal(CustomerSyncOperation.Create, events[0].Operation);
        Assert.Equal(CustomerSyncOperation.Update, events[1].Operation);

        // Both point at the same customer, which is what makes the update land on the
        // record the create made.
        Assert.Equal(events[0].CustomerId, events[1].CustomerId);
        Assert.Equal(12_500m, events[1].RequestedAmount);
    }

    [Fact]
    public async Task Keeps_the_events_unprocessed_until_the_background_worker_takes_them()
    {
        using var factory = new LoansApiFactory();
        using var client = factory.CreateClient();

        await client.PostAsJsonAsync(Endpoint, LoanApplicationRequests.Valid(ssn: Ssn));

        var message = await factory.QueryAsync(context => context.OutboxMessages.SingleAsync());

        Assert.Null(message.ProcessedAt);
        Assert.Equal(0, message.AttemptCount);
        Assert.Equal(nameof(CustomerSyncRequested), message.Type);
    }
}
