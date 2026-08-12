using System.Security.Cryptography;
using System.Text;
using Fundo.Loans.Application.Abstractions;
using Fundo.Loans.Domain.Customers;
using Microsoft.Extensions.Options;

namespace Fundo.Loans.Infrastructure.Security;

/// <summary>
/// Hashes an SSN with HMAC-SHA256 under a configured key.
/// </summary>
/// <remarks>
/// Deterministic, because the hash has to be searchable to find a returning customer.
/// Keyed rather than a bare SHA-256: the search space of nine digits is small enough
/// that an unkeyed digest of every possible SSN can be precomputed in minutes, so the
/// key is what makes a stolen database column useless on its own.
/// </remarks>
internal sealed class HmacSsnHasher : ISsnHasher
{
    private readonly byte[] _key;

    public HmacSsnHasher(IOptions<SsnHashingOptions> options)
    {
        var key = options.Value.Key;
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException(
                $"{nameof(SsnHashingOptions)}.{nameof(SsnHashingOptions.Key)} must be configured.");
        }

        _key = Encoding.UTF8.GetBytes(key);
    }

    public string Hash(Ssn ssn)
    {
        var digest = HMACSHA256.HashData(_key, Encoding.UTF8.GetBytes(ssn.Digits));
        return Convert.ToHexStringLower(digest);
    }
}
