using Fundo.Loans.Domain.Customers;

namespace Fundo.Loans.Domain.Decisions;

/// <summary>
/// Everything the rules are allowed to look at. Deliberately separate from
/// <see cref="Customer"/>: a decision is made before anything is persisted, and
/// rules should not be able to reach into stored state.
/// </summary>
public sealed record Applicant(
    string FirstName,
    string LastName,
    Address Address,
    string CompanyName,
    decimal RequestedAmount,
    Ssn Ssn);
