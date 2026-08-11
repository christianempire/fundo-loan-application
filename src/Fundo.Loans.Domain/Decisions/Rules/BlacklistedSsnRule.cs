namespace Fundo.Loans.Domain.Decisions.Rules;

/// <summary>
/// Denies applicants whose SSN is on the blacklist.
/// </summary>
public sealed class BlacklistedSsnRule : IDenialRule
{
    public const string Code = "BLACKLISTED_SSN";

    private readonly ISsnBlacklist _blacklist;

    public BlacklistedSsnRule(ISsnBlacklist blacklist) => _blacklist = blacklist;

    public Denial? Evaluate(Applicant applicant) =>
        _blacklist.Contains(applicant.Ssn)
            ? new Denial(Code, "We are unable to approve this application.")
            : null;
}
