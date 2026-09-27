using MechanicShop.Domain.Customers.Vehicles;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Infrastructure.Data.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.HasKey(v => v.Id)
            .IsClustered(false);

        builder.Property(v => v.Id)
            .ValueGeneratedNever();

        builder.Property(v => v.Make)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.Model)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.LicensePlate)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(v => v.Year)
            .IsRequired();

        builder.HasOne(v => v.Customer)
            .WithMany(c => c.Vehicles)
            .HasForeignKey(v => v.CustomerId);
    }
}
