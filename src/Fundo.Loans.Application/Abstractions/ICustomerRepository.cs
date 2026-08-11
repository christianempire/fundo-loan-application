using Fundo.Loans.Domain.Customers;

namespace Fundo.Loans.Application.Abstractions;

public interface ICustomerRepository
{
    /// <summary>The returning-customer lookup. The hash is the natural key.</summary>
    Task<Customer?> FindBySsnHashAsync(string ssnHash, CancellationToken cancellationToken);

    void Add(Customer customer);
}
