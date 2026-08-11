using Fundo.Loans.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Fundo.Loans.Infrastructure.Persistence;

/// <summary>
/// Wraps a block of work in one database transaction.
/// </summary>
/// <remarks>
/// Everything the work queued up is saved once, at the end, and committed together.
/// If the work throws — including the outbox write or anything the handler does after
/// it — the transaction is rolled back and the database keeps no trace of the attempt.
/// The retry strategy is asked to re-run the whole block, not just the save, because
/// the block is the unit that has to be atomic.
/// </remarks>
internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly LoansDbContext _context;

    public UnitOfWork(LoansDbContext context) => _context = context;

    public Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> work,
        CancellationToken cancellationToken)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        return strategy.ExecuteAsync(async token =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(token);

            await work(token);

            await _context.SaveChangesAsync(token);
            await transaction.CommitAsync(token);
        }, cancellationToken);
    }
}
