using System.Net;
using Fundo.Loans.Application.IntegrationEvents;
using Fundo.Loans.Infrastructure.ExternalService;

namespace Fundo.Loans.Tests.Infrastructure;

public class HttpCustomerSyncClientTests
{
    private static CustomerSyncRequested Event(CustomerSyncOperation operation, Guid customerId) =>
        new(
            operation,
            customerId,
            FirstName: "Ada",
            LastName: "Lovelace",
            CompanyName: "Analytical Engines",
            Address: new CustomerSyncAddress("742 Evergreen Terrace", "San Diego", "CA", "92101"),
            SsnLast4: "6666",
            ApplicationId: Guid.CreateVersion7(),
            RequestedAmount: 5_000m);

    [Fact]
    public async Task Posts_to_the_collection_when_the_customer_is_new()
    {
        var handler = new RecordingHandler(HttpStatusCode.OK);
        var client = ClientFor(handler);

        await client.SendAsync(Event(CustomerSyncOperation.Create, Guid.CreateVersion7()), CancellationToken.None);

        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("/customers", handler.Path);
    }

    [Fact]
    public async Task Puts_to_the_customer_when_they_are_returning()
    {
        var handler = new RecordingHandler(HttpStatusCode.OK);
        var client = ClientFor(handler);
        var customerId = Guid.CreateVersion7();

        await client.SendAsync(Event(CustomerSyncOperation.Update, customerId), CancellationToken.None);

        Assert.Equal(HttpMethod.Put, handler.Method);
        Assert.Equal($"/customers/{customerId}", handler.Path);
    }

    [Fact]
    public async Task Sends_only_the_last_four_digits_of_the_ssn()
    {
        var handler = new RecordingHandler(HttpStatusCode.OK);
        var client = ClientFor(handler);

        await client.SendAsync(Event(CustomerSyncOperation.Create, Guid.CreateVersion7()), CancellationToken.None);

        Assert.Contains("\"ssnLast4\":\"6666\"", handler.Body);
        Assert.DoesNotContain("444556666", handler.Body);
    }

    [Fact]
    public async Task Throws_when_the_external_service_rejects_the_call_so_the_outbox_can_retry()
    {
        var handler = new RecordingHandler(HttpStatusCode.ServiceUnavailable);
        var client = ClientFor(handler);

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            client.SendAsync(Event(CustomerSyncOperation.Create, Guid.CreateVersion7()), CancellationToken.None));
    }

    private static HttpCustomerSyncClient ClientFor(HttpMessageHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("http://external-service.test") });

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;

        public RecordingHandler(HttpStatusCode statusCode) => _statusCode = statusCode;

        public HttpMethod? Method { get; private set; }

        public string? Path { get; private set; }

        public string Body { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Method = request.Method;
            Path = request.RequestUri?.AbsolutePath;
            Body = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(_statusCode);
        }
    }
}
