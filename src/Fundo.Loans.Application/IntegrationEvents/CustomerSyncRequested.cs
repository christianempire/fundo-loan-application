namespace Fundo.Loans.Application.IntegrationEvents;

public enum CustomerSyncOperation
{
    Create = 1,
    Update = 2,
}

/// <summary>
/// Raised when an approved application has been written, so the external service
/// can be brought in line with it.
/// </summary>
/// <remarks>
/// The event carries a full snapshot rather than only identifiers. The handler runs
/// later, outside the request, and must not depend on the database still holding the
/// same values by then. The SSN is deliberately reduced to its last four digits: the
/// external service has no need for the rest.
/// </remarks>
public sealed record CustomerSyncRequested(
    CustomerSyncOperation Operation,
    Guid CustomerId,
    string FirstName,
    string LastName,
    string CompanyName,
    CustomerSyncAddress Address,
    string SsnLast4,
    Guid ApplicationId,
    decimal RequestedAmount);

public sealed record CustomerSyncAddress(string Street, string City, string State, string PostalCode);
