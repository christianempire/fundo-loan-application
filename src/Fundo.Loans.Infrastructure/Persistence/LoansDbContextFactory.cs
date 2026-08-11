using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Fundo.Loans.Infrastructure.Persistence;

/// <summary>
/// Used only by <c>dotnet ef</c> when scaffolding migrations, so the tool does not
/// need to boot the API to see the model. The connection string is irrelevant here:
/// migrations are generated from the model, not from a live database.
/// </summary>
public sealed class LoansDbContextFactory : IDesignTimeDbContextFactory<LoansDbContext>
{
    public LoansDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<LoansDbContext>()
            .UseSqlite("Data Source=design-time.db")
            .Options;

        return new LoansDbContext(options);
    }
}
