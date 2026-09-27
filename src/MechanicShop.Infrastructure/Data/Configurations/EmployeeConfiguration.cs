using MechanicShop.Domain.Employees;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Infrastructure.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(e => e.Id)
            .IsClustered(false);

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Role)
            .HasConversion<string>()
            .IsRequired();
    }
}
