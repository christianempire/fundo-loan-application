using Fundo.Loans.Domain.Customers;
using Fundo.Loans.Domain.Decisions;

namespace Fundo.Loans.Application.LoanApplications;

/// <summary>
/// A submitted form, already parsed into domain types by the edge that received it.
/// </summary>
public sealed record SubmitLoanApplicationCommand(
    string FirstName,
    string LastName,
    string CompanyName,
    Address Address,
    decimal RequestedAmount,
    Ssn Ssn);

/// <summary>The outcome the caller needs: approved with an id, or denied with a reason.</summary>
public sealed record SubmitLoanApplicationResult
{
    private SubmitLoanApplicationResult(Guid? applicationId, Denial? denial)
    {
        ApplicationId = applicationId;
        Denial = denial;
    }

    public Guid? ApplicationId { get; }

    public Denial? Denial { get; }

    public bool IsApproved => Denial is null;

    public static SubmitLoanApplicationResult Approved(Guid applicationId) => new(applicationId, denial: null);

    public static SubmitLoanApplicationResult Denied(Denial denial) => new(applicationId: null, denial);
}
