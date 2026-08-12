using System.Net;
using System.Net.Http.Json;
using Fundo.Loans.Application.Abstractions;
using Fundo.Loans.Application.IntegrationEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fundo.Loans.Tests.Integration;

/// <summary>
/// Saving the customer, saving the application and publishing the event are one unit
/// of work. These tests break the last of the three and check that the first two are
/// undone with it.
/// </summary>
public class TransactionalWriteTests
{
    private const string Endpoint = "/api/loan-applications";

    [Fact]
    public async Task Rolls_the_whole_write_back_when_publishing_fails()
    {
        using var factory = new LoansApiFactory(services =>
            services.Replace(ServiceDescriptor.Scoped<IIntegrationEventPublisher, FailingPublisher>()));
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(Endpoint, LoanApplicationRequests.Valid());

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        // No half-saved customer, no orphan application, no event.
        Assert.Equal(0, await factory.QueryAsync(context => context.Customers.CountAsync()));
        Assert.Equal(0, await factory.QueryAsync(context => context.LoanApplications.CountAsync()));
        Assert.Equal(0, await factory.QueryAsync(context => context.OutboxMessages.CountAsync()));
    }

    [Fact]
    public async Task Leaves_an_existing_customer_untouched_when_a_later_submission_fails()
    {
        var publisher = new SwitchablePublisher();

        using var factory = new LoansApiFactory(services =>
            services.Replace(ServiceDescriptor.Singleton<IIntegrationEventPublisher>(publisher)));
        using var client = factory.CreateClient();

        await client.PostAsJsonAsync(Endpoint, LoanApplicationRequests.Valid(
            ssn: "444-55-6666", amount: 5_000m, companyName: "Analytical Engines"));

        publisher.Fail = true;

        var response = await client.PostAsJsonAsync(Endpoint, LoanApplicationRequests.Valid(
            ssn: "444-55-6666", amount: 99_000m, companyName: "Should Not Persist"));

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        // The update is rolled back too: the returning-customer path is a write like any other.
        var customer = await factory.QueryAsync(context => context.Customers.SingleAsync());
        var application = await factory.QueryAsync(context => context.LoanApplications.SingleAsync());

        Assert.Equal("Analytical Engines", customer.CompanyName);
        Assert.Equal(5_000m, application.RequestedAmount);
    }

    private sealed class FailingPublisher : IIntegrationEventPublisher
    {
        public Task PublishAsync(CustomerSyncRequested @event, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("The event could not be published.");
    }

    private sealed class SwitchablePublisher : IIntegrationEventPublisher
    {
        public bool Fail { get; set; }

        public Task PublishAsync(CustomerSyncRequested @event, CancellationToken cancellationToken) =>
            Fail
                ? throw new InvalidOperationException("The event could not be published.")
                : Task.CompletedTask;
    }
}
