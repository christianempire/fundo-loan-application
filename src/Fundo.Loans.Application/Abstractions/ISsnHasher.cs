using Fundo.Loans.Domain.Customers;

namespace Fundo.Loans.Application.Abstractions;

/// <summary>
/// Turns an SSN into the deterministic hash stored against a customer. Deterministic
/// on purpose: the hash has to be searchable, since it replaces the SSN as the key.
/// </summary>
public interface ISsnHasher
{
    string Hash(Ssn ssn);
}
