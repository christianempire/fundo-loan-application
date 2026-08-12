using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Fundo.Loans.Tests.Integration;

public class SubmitLoanApplicationTests
{
    private const string Endpoint = "/api/loan-applications";

    [Fact]
    public async Task Approves_a_clean_application_and_records_it()
    {
        using var factory = new LoansApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(Endpoint, LoanApplicationRequests.Valid());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Approved", body.GetProperty("decision").GetString());
        Assert.NotEqual(Guid.Empty, body.GetProperty("applicationId").GetGuid());

        var customers = await factory.QueryAsync(context => context.Customers.CountAsync());
        var applications = await factory.QueryAsync(context => context.LoanApplications.CountAsync());

        Assert.Equal(1, customers);
        Assert.Equal(1, applications);
    }

    [Fact]
    public async Task Never_stores_the_ssn_itself()
    {
        using var factory = new LoansApiFactory();
        using var client = factory.CreateClient();

        await client.PostAsJsonAsync(Endpoint, LoanApplicationRequests.Valid(ssn: "444-55-6666"));

        var customer = await factory.QueryAsync(context => context.Customers.SingleAsync());

        Assert.Equal("6666", customer.SsnLast4);
        Assert.DoesNotContain("444556666", customer.SsnHash);
        Assert.Equal(64, customer.SsnHash.Length);
    }

    [Theory]
    [InlineData("NY", "RESTRICTED_STATE")]
    public async Task Denies_a_restricted_state_without_writing_anything(string state, string expectedCode)
    {
        using var factory = new LoansApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(Endpoint, LoanApplicationRequests.Valid(state: state));
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Denied", body.GetProperty("decision").GetString());
        Assert.Equal(expectedCode, body.GetProperty("denialCode").GetString());

        Assert.Equal(0, await factory.QueryAsync(context => context.Customers.CountAsync()));
        Assert.Equal(0, await factory.QueryAsync(context => context.OutboxMessages.CountAsync()));
    }

    [Fact]
    public async Task Denies_a_blacklisted_ssn_without_writing_anything()
    {
        using var factory = new LoansApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(Endpoint, LoanApplicationRequests.Valid(ssn: "111-11-1111"));
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal("Denied", body.GetProperty("decision").GetString());
        Assert.Equal("BLACKLISTED_SSN", body.GetProperty("denialCode").GetString());

        Assert.Equal(0, await factory.QueryAsync(context => context.Customers.CountAsync()));
    }

    [Fact]
    public async Task Rejects_a_malformed_form_with_field_level_errors()
    {
        using var factory = new LoansApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(Endpoint, new
        {
            firstName = "",
            lastName = "Lovelace",
            companyName = "Analytical Engines",
            address = new { street = "1 Main", city = "San Diego", state = "California", postalCode = "9210" },
            requestedAmount = 0,
            ssn = "12345",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        var errors = problem.GetProperty("errors");

        Assert.True(errors.TryGetProperty("FirstName", out _));
        Assert.True(errors.TryGetProperty("RequestedAmount", out _));
        Assert.True(errors.TryGetProperty("Ssn", out _));
        Assert.True(errors.TryGetProperty("Address.State", out _));
        Assert.True(errors.TryGetProperty("Address.PostalCode", out _));
    }
}
