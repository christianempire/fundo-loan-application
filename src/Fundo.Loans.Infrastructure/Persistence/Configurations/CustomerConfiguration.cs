using Fundo.Loans.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fundo.Loans.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(customer => customer.Id);

        // One customer per SSN is the rule the returning-customer flow depends on,
        // so the database enforces it rather than trusting the application to.
        builder.HasIndex(customer => customer.SsnHash).IsUnique();

        builder.Property(customer => customer.SsnHash).HasMaxLength(64).IsRequired();
        builder.Property(customer => customer.SsnLast4).HasMaxLength(4).IsRequired();
        builder.Property(customer => customer.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(customer => customer.LastName).HasMaxLength(100).IsRequired();
        builder.Property(customer => customer.CompanyName).HasMaxLength(200).IsRequired();

        builder.ComplexProperty(customer => customer.Address, address =>
        {
            address.Property(value => value.Street).HasColumnName("street").HasMaxLength(200).IsRequired();
            address.Property(value => value.City).HasColumnName("city").HasMaxLength(100).IsRequired();
            address.Property(value => value.State).HasColumnName("state").HasMaxLength(2).IsRequired();
            address.Property(value => value.PostalCode).HasColumnName("postal_code").HasMaxLength(10).IsRequired();
        });
    }
}
