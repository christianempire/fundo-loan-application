namespace Fundo.Loans.Infrastructure.Decisions;

/// <summary>
/// The data the deny rules work from. Kept in configuration so changing who gets
/// denied is a deployment concern, not a code change.
/// </summary>
public sealed class DecisionRulesOptions
{
    public const string SectionName = "DecisionRules";

    /// <summary>Two-letter state codes Fundo does not lend in.</summary>
    public IReadOnlyList<string> RestrictedStates { get; set; } = [];

    public IReadOnlyList<string> BlacklistedSsns { get; set; } = [];
}
