using MechanicShop.Domain.Customers;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id)
            .IsClustered(false);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder.Navigation(c => c.Vehicles)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
