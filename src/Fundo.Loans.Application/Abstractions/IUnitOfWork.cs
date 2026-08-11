namespace Fundo.Loans.Application.Abstractions;

/// <summary>
/// Runs a block of work as a single database transaction, committing when it
/// returns and rolling back if it throws.
/// </summary>
public interface IUnitOfWork
{
    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> work,
        CancellationToken cancellationToken);
}
