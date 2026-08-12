using Fundo.Loans.Application.LoanApplications;
using Fundo.Loans.Domain.Customers;

namespace Fundo.Loans.Api.LoanApplications;

public static class LoanApplicationEndpoints
{
    public static IEndpointRouteBuilder MapLoanApplications(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/loan-applications", SubmitAsync)
            .WithName("SubmitLoanApplication")
            .WithSummary("Evaluates a loan application and, if approved, records it.");

        return routes;
    }

    /// <summary>
    /// Validates the shape, maps it into the domain and hands it to the use case. All
    /// of the deciding, writing and publishing happens behind that one call.
    /// </summary>
    private static async Task<IResult> SubmitAsync(
        SubmitLoanApplicationRequest request,
        SubmitLoanApplicationHandler handler,
        CancellationToken cancellationToken)
    {
        if (!RequestValidator.TryValidate(request, out var errors))
        {
            return Results.ValidationProblem(errors);
        }

        if (!Ssn.TryParse(request.Ssn, out var ssn))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.Ssn)] = ["The SSN must be nine digits, for example 123-45-6789."],
            });
        }

        var command = new SubmitLoanApplicationCommand(
            request.FirstName,
            request.LastName,
            request.CompanyName,
            new Address(
                request.Address.Street,
                request.Address.City,
                request.Address.State,
                request.Address.PostalCode),
            request.RequestedAmount,
            ssn);

        var result = await handler.HandleAsync(command, cancellationToken);

        return Results.Ok(result.IsApproved
            ? SubmitLoanApplicationResponse.Approved(result.ApplicationId!.Value)
            : SubmitLoanApplicationResponse.Denied(result.Denial!.Code, result.Denial.Reason));
    }
}
