namespace Fundo.Loans.Domain.Decisions;

/// <summary>
/// One reason to turn an application down.
/// </summary>
/// <remarks>
/// A rule answers about itself and nothing else: it returns a <see cref="Denial"/>
/// when it objects, or <c>null</c> when it has no opinion. Adding a reason to deny
/// means adding an implementation and registering it — no existing rule and no
/// part of <see cref="DecisionEngine"/> changes.
/// </remarks>
public interface IDenialRule
{
    Denial? Evaluate(Applicant applicant);
}
