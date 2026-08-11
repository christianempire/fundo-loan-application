using Fundo.Loans.Domain.Applications;
using Fundo.Loans.Domain.Customers;
using Fundo.Loans.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Fundo.Loans.Infrastructure.Persistence;

public sealed class LoansDbContext : DbContext
{
    public LoansDbContext(DbContextOptions<LoansDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<LoanApplication> LoanApplications => Set<LoanApplication>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LoansDbContext).Assembly);
}
