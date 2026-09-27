using MechanicShop.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(rt => rt.Id)
            .IsClustered(false);

        builder.HasIndex(rt => rt.Token)
            .IsUnique();

        builder.Property(rt => rt.UserId)
            .IsRequired();

        builder.Property(rt => rt.ExpiresAtUtc)
            .IsRequired();

        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(200);
    }
}