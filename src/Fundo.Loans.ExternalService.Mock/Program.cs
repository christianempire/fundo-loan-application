using System.Collections.Concurrent;
using Fundo.Loans.ExternalService.Mock;

// A stand-in for the partner system the loan data is pushed to. It keeps records in
// memory, answers 200, and exposes a GET so the sync can be seen to have happened.
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var customers = new ConcurrentDictionary<Guid, SyncedCustomer>();

app.MapPost("/customers", (SyncedCustomer customer, ILogger<Program> logger) =>
{
    // Create is idempotent on the reference so a retried delivery is harmless.
    customers[customer.CustomerId] = customer;
    logger.LogInformation("Created customer {CustomerId} ({Last4})", customer.CustomerId, customer.SsnLast4);

    return Results.Ok(new { received = true, operation = "created", customer.CustomerId });
});

app.MapPut("/customers/{customerId:guid}", (Guid customerId, SyncedCustomer customer, ILogger<Program> logger) =>
{
    customers[customerId] = customer with { CustomerId = customerId };
    logger.LogInformation("Updated customer {CustomerId} ({Last4})", customerId, customer.SsnLast4);

    return Results.Ok(new { received = true, operation = "updated", customerId });
});

app.MapGet("/customers", () => Results.Ok(customers.Values.OrderBy(customer => customer.LastName)));

app.MapGet("/customers/{customerId:guid}", (Guid customerId) =>
    customers.TryGetValue(customerId, out var customer) ? Results.Ok(customer) : Results.NotFound());

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
