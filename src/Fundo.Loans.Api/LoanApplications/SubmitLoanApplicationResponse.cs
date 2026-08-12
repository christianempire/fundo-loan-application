namespace Fundo.Loans.Api.LoanApplications;

/// <summary>
/// What the form gets back. A denial is a normal outcome of a valid request, not an
/// error, so it comes back as 200 with a decision the client can branch on.
/// </summary>
public sealed record SubmitLoanApplicationResponse(
    string Decision,
    Guid? ApplicationId,
    string? DenialCode,
    string? DenialReason)
{
    public const string ApprovedDecision = "Approved";
    public const string DeniedDecision = "Denied";

    public static SubmitLoanApplicationResponse Approved(Guid applicationId) =>
        new(ApprovedDecision, applicationId, DenialCode: null, DenialReason: null);

    public static SubmitLoanApplicationResponse Denied(string code, string reason) =>
        new(DeniedDecision, ApplicationId: null, code, reason);
}
