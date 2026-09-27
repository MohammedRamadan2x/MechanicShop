using MechanicShop.Domain.RepairTasks;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Infrastructure.Data.Configurations;

public class RepairTaskConfiguration : IEntityTypeConfiguration<RepairTask>
{
    public void Configure(EntityTypeBuilder<RepairTask> builder)
    {
        builder.HasKey(rt => rt.Id)
            .IsClustered(false);

        builder.Property(rt => rt.Id)
            .ValueGeneratedNever();

        builder.Property(rt => rt.Name)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(rt => rt.EstimatedDurationInMins)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(rt => rt.LaborCost)
               .HasPrecision(18, 2)
               .IsRequired();

        builder.HasMany(rt => rt.Parts)
           .WithOne()
           .HasForeignKey("RepairTaskId")
           .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(rt => rt.Parts)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}