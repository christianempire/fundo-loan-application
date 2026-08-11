using Fundo.Loans.Domain.Customers;

namespace Fundo.Loans.Domain.Decisions.Rules;

/// <summary>
/// The set of SSNs Fundo refuses to lend to. Where that set comes from is an
/// infrastructure concern; the rule only needs to ask.
/// </summary>
public interface ISsnBlacklist
{
    bool Contains(Ssn ssn);
}
