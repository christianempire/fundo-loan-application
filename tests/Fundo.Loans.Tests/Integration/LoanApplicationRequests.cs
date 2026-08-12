namespace Fundo.Loans.Tests.Integration;

/// <summary>Request bodies for the endpoint tests, built as anonymous JSON.</summary>
internal static class LoanApplicationRequests
{
    public static object Valid(
        string ssn = "444-55-6666",
        string state = "CA",
        decimal amount = 5_000m,
        string firstName = "Ada",
        string lastName = "Lovelace",
        string companyName = "Analytical Engines") =>
        new
        {
            firstName,
            lastName,
            companyName,
            address = new
            {
                street = "742 Evergreen Terrace",
                city = "San Diego",
                state,
                postalCode = "92101",
            },
            requestedAmount = amount,
            ssn,
        };
}
