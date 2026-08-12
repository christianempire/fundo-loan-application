using System.ComponentModel.DataAnnotations;

namespace Fundo.Loans.Api.LoanApplications;

/// <summary>
/// The form as it arrives over the wire. Shape is validated here, at the edge, so
/// nothing malformed reaches the domain.
/// </summary>
public sealed record SubmitLoanApplicationRequest
{
    [Required, StringLength(100, MinimumLength = 1)]
    public string FirstName { get; init; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 1)]
    public string LastName { get; init; } = string.Empty;

    [Required, StringLength(200, MinimumLength = 1)]
    public string CompanyName { get; init; } = string.Empty;

    [Required]
    public AddressRequest Address { get; init; } = new();

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "The requested amount must be greater than zero.")]
    public decimal RequestedAmount { get; init; }

    /// <summary>Nine digits, with or without dashes. Parsed, never stored as given.</summary>
    [Required]
    [RegularExpression(@"^\d{3}-?\d{2}-?\d{4}$", ErrorMessage = "The SSN must be nine digits, for example 123-45-6789.")]
    public string Ssn { get; init; } = string.Empty;
}

public sealed record AddressRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Street { get; init; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 1)]
    public string City { get; init; } = string.Empty;

    [Required]
    [RegularExpression("^[A-Za-z]{2}$", ErrorMessage = "The state must be a two-letter code, for example CA.")]
    public string State { get; init; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "The ZIP code must be five digits, optionally followed by a four-digit extension.")]
    public string PostalCode { get; init; } = string.Empty;
}
