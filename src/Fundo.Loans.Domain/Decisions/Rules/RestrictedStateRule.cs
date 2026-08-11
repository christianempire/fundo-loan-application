using Fundo.Loans.Domain.Customers;

namespace Fundo.Loans.Domain.Decisions.Rules;

/// <summary>
/// Fundo does not lend in every state. Applicants from a restricted one are denied.
/// </summary>
public sealed class RestrictedStateRule : IDenialRule
{
    public const string Code = "RESTRICTED_STATE";

    private readonly IReadOnlySet<string> _restrictedStates;

    public RestrictedStateRule(IEnumerable<string> restrictedStates) =>
        _restrictedStates = restrictedStates
            .Select(state => state.Trim().ToUpperInvariant())
            .ToHashSet();

    public Denial? Evaluate(Applicant applicant) =>
        _restrictedStates.Contains(applicant.Address.State)
            ? new Denial(Code, $"We are not lending in {applicant.Address.State} at the moment.")
            : null;
}
