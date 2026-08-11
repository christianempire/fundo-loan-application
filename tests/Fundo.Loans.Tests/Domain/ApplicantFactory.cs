using Fundo.Loans.Domain.Customers;
using Fundo.Loans.Domain.Decisions;

namespace Fundo.Loans.Tests.Domain;

/// <summary>Builds an applicant that passes every rule, so a test can spoil one thing.</summary>
internal static class ApplicantFactory
{
    public static Applicant Create(string state = "CA", string ssn = "111-22-3333") =>
        new(
            FirstName: "Ada",
            LastName: "Lovelace",
            Address: new Address("742 Evergreen Terrace", "San Diego", state, "92101"),
            CompanyName: "Analytical Engines",
            RequestedAmount: 5_000m,
            Ssn: Ssn.Parse(ssn));
}
