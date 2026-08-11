namespace Fundo.Loans.Domain.Decisions;

/// <summary>Why an application was turned down.</summary>
/// <param name="Code">Stable identifier, safe to branch on.</param>
/// <param name="Reason">Human-readable text.</param>
public sealed record Denial(string Code, string Reason);

/// <summary>The outcome of running an applicant through the rules.</summary>
public sealed record Decision
{
    private Decision(Denial? denial) => Denial = denial;

    public Denial? Denial { get; }

    public bool IsApproved => Denial is null;

    public static Decision Approve() => new(denial: null);

    public static Decision Deny(Denial denial) => new(denial);
}
