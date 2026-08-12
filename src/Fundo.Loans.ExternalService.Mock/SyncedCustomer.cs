namespace Fundo.Loans.ExternalService.Mock;

/// <summary>
/// The contract this service accepts. It mirrors what the outbox processor sends and
/// is intentionally declared here, separately from the sending side: the two are
/// different systems that happen to agree on a payload.
/// </summary>
public sealed record SyncedCustomer(
    Guid CustomerId,
    string FirstName,
    string LastName,
    string CompanyName,
    SyncedAddress Address,
    string SsnLast4,
    Guid ApplicationId,
    decimal RequestedAmount);

public sealed record SyncedAddress(string Street, string City, string State, string PostalCode);
