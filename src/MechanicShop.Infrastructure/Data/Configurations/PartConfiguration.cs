using MechanicShop.Domain.RepairTasks.Parts;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Infrastructure.Data.Configurations;

public class PartConfiguration : IEntityTypeConfiguration<Part>
{
    public void Configure(EntityTypeBuilder<Part> builder)
    {
        builder.HasKey(p => p.Id)
            .IsClustered(false);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Cost)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.Quantity)
            .IsRequired();
    }
}