namespace Fundo.Loans.Domain.Customers;

/// <summary>
/// A person who applied for a loan. Identified across submissions by their SSN,
/// which is stored only as a hash plus the last four digits.
/// </summary>
public sealed class Customer
{
    private Customer()
    {
        // Rehydration by EF Core.
        SsnHash = null!;
        SsnLast4 = null!;
        FirstName = null!;
        LastName = null!;
        CompanyName = null!;
        Address = null!;
    }

    private Customer(string ssnHash, string ssnLast4)
        : this()
    {
        Id = Guid.CreateVersion7();
        SsnHash = ssnHash;
        SsnLast4 = ssnLast4;
        RegisteredAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    /// <summary>Deterministic hash of the SSN. The natural key for a returning customer.</summary>
    public string SsnHash { get; private set; }

    public string SsnLast4 { get; private set; }

    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public string CompanyName { get; private set; }

    public Address Address { get; private set; }

    public DateTime RegisteredAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Customer Register(
        string ssnHash,
        string ssnLast4,
        string firstName,
        string lastName,
        string companyName,
        Address address)
    {
        var customer = new Customer(ssnHash, ssnLast4);
        customer.UpdateDetails(firstName, lastName, companyName, address);
        return customer;
    }

    /// <summary>
    /// Overwrites the personal data with the latest submission. The SSN itself is
    /// the identity of the record and never changes here.
    /// </summary>
    public void UpdateDetails(string firstName, string lastName, string companyName, Address address)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        CompanyName = companyName.Trim();
        Address = address;
        UpdatedAt = DateTime.UtcNow;
    }
}
