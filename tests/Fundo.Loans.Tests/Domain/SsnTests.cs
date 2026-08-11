using Fundo.Loans.Domain.Customers;

namespace Fundo.Loans.Tests.Domain;

public class SsnTests
{
    [Theory]
    [InlineData("123-45-6789")]
    [InlineData("123456789")]
    [InlineData(" 123 45 6789 ")]
    public void Parses_and_normalizes_accepted_formats(string input)
    {
        Assert.True(Ssn.TryParse(input, out var ssn));
        Assert.Equal("123456789", ssn.Digits);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("12345678")]
    [InlineData("1234567890")]
    [InlineData("12345678A")]
    public void Rejects_anything_that_is_not_nine_digits(string? input)
    {
        Assert.False(Ssn.TryParse(input, out _));
    }

    [Fact]
    public void Exposes_only_the_last_four_digits()
    {
        var ssn = Ssn.Parse("123-45-6789");

        Assert.Equal("6789", ssn.Last4);
        Assert.Equal("***-**-6789", ssn.ToString());
    }

    [Fact]
    public void Treats_the_same_number_written_differently_as_equal()
    {
        Assert.Equal(Ssn.Parse("123-45-6789"), Ssn.Parse("123456789"));
    }
}
