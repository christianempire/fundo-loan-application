using Fundo.Loans.Domain.Applications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fundo.Loans.Infrastructure.Persistence.Configurations;

internal sealed class LoanApplicationConfiguration : IEntityTypeConfiguration<LoanApplication>
{
    public void Configure(EntityTypeBuilder<LoanApplication> builder)
    {
        builder.ToTable("loan_applications");

        builder.HasKey(application => application.Id);

        // "Same SSN means one customer and one application" — enforced here too.
        builder.HasIndex(application => application.CustomerId).IsUnique();

        builder.Property(application => application.RequestedAmount)
            .HasPrecision(18, 2)
            .IsRequired();
    }
}
