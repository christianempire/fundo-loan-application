using Fundo.Loans.Application.Abstractions;
using Fundo.Loans.Domain.Applications;
using Microsoft.EntityFrameworkCore;

namespace Fundo.Loans.Infrastructure.Persistence.Repositories;

internal sealed class LoanApplicationRepository : ILoanApplicationRepository
{
    private readonly LoansDbContext _context;

    public LoanApplicationRepository(LoansDbContext context) => _context = context;

    public Task<LoanApplication?> FindByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken) =>
        _context.LoanApplications.SingleOrDefaultAsync(
            application => application.CustomerId == customerId, cancellationToken);

    public void Add(LoanApplication application) => _context.LoanApplications.Add(application);
}
