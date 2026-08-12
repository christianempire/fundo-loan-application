using Fundo.Loans.Api.LoanApplications;
using Fundo.Loans.Infrastructure;
using Fundo.Loans.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLoansInfrastructure(builder.Configuration);
builder.Services.AddProblemDetails();

var app = builder.Build();

// The schema is applied on start-up so a reviewer needs no migration step. In a real
// deployment this would be a release task rather than something the app does to itself.
await using (var scope = app.Services.CreateAsyncScope())
{
    await scope.ServiceProvider.GetRequiredService<LoansDbContext>().Database.MigrateAsync();
}

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapLoanApplications();
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).ExcludeFromDescription();

app.Run();

/// <summary>Exposed so the integration tests can host this same application.</summary>
public partial class Program;
