using Fundo.Loans.Domain.Applications;

namespace Fundo.Loans.Application.Abstractions;

public interface ILoanApplicationRepository
{
    /// <summary>A customer holds at most one application, so this returns it or nothing.</summary>
    Task<LoanApplication?> FindByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken);

    void Add(LoanApplication application);
}
