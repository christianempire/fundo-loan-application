using Fundo.Loans.Application.Abstractions;
using Fundo.Loans.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace Fundo.Loans.Infrastructure.Persistence.Repositories;

internal sealed class CustomerRepository : ICustomerRepository
{
    private readonly LoansDbContext _context;

    public CustomerRepository(LoansDbContext context) => _context = context;

    public Task<Customer?> FindBySsnHashAsync(string ssnHash, CancellationToken cancellationToken) =>
        _context.Customers.SingleOrDefaultAsync(customer => customer.SsnHash == ssnHash, cancellationToken);

    public void Add(Customer customer) => _context.Customers.Add(customer);
}
