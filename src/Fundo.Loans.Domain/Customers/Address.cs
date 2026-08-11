namespace Fundo.Loans.Domain.Customers;

/// <summary>
/// A US postal address. <see cref="State"/> is the two-letter code and is the
/// only part the decision rules look at today.
/// </summary>
public sealed record Address(string Street, string City, string State, string PostalCode)
{
    public string State { get; } = State.Trim().ToUpperInvariant();

    public override string ToString() => $"{Street}, {City}, {State} {PostalCode}";
}
