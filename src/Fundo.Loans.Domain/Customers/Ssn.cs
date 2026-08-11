using System.Text.RegularExpressions;

namespace Fundo.Loans.Domain.Customers;

/// <summary>
/// A US Social Security Number, normalized to nine digits.
/// </summary>
/// <remarks>
/// The raw value never leaves the request that carried it: it is used to match a
/// returning customer and to evaluate the blacklist rule, and only its hash and
/// last four digits are persisted. See <c>ISsnHasher</c>.
/// </remarks>
public sealed partial record Ssn
{
    private Ssn(string digits) => Digits = digits;

    /// <summary>The nine digits, without separators.</summary>
    public string Digits { get; }

    public string Last4 => Digits[^4..];

    public static bool TryParse(string? value, out Ssn ssn)
    {
        ssn = null!;
        if (string.IsNullOrWhiteSpace(value)) return false;

        var digits = SeparatorPattern().Replace(value.Trim(), string.Empty);
        if (!NineDigitsPattern().IsMatch(digits)) return false;

        ssn = new Ssn(digits);
        return true;
    }

    public static Ssn Parse(string value) => TryParse(value, out var ssn)
        ? ssn
        : throw new ArgumentException($"'{value}' is not a valid SSN.", nameof(value));

    public override string ToString() => $"***-**-{Last4}";

    [GeneratedRegex(@"[\s-]")]
    private static partial Regex SeparatorPattern();

    [GeneratedRegex(@"^\d{9}$")]
    private static partial Regex NineDigitsPattern();
}
