namespace Fundo.Loans.Domain.Decisions;

/// <summary>
/// Runs an applicant past every denial rule. The first rule that objects wins;
/// if none does, the application is approved.
/// </summary>
public sealed class DecisionEngine
{
    private readonly IReadOnlyCollection<IDenialRule> _rules;

    public DecisionEngine(IEnumerable<IDenialRule> rules) => _rules = [.. rules];

    public Decision Evaluate(Applicant applicant)
    {
        ArgumentNullException.ThrowIfNull(applicant);

        foreach (var rule in _rules)
        {
            if (rule.Evaluate(applicant) is { } denial)
            {
                return Decision.Deny(denial);
            }
        }

        return Decision.Approve();
    }
}
