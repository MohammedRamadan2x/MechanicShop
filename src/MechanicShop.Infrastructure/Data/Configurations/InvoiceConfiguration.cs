using MechanicShop.Domain.WorkOrders.Billing;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Infrastructure.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(i => i.Id)
            .IsClustered(false);

        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.Property(i => i.PaidAt);
        
        builder.Property(i => i.IssuedAtUtc)
            .IsRequired();
        
        builder.Property(i => i.DiscountAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(i => i.TaxAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(i => i.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Navigation(i => i.LineItems)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(i => i.LineItems, items =>
        {
            items.ToTable("InvoiceLineItems");

            items.WithOwner().HasForeignKey(i => i.InvoiceId);

            items.HasKey(li => new { li.InvoiceId, li.LineNumber} );

            items.Property(li => li.LineNumber)
                .ValueGeneratedNever();

            items.Property(li => li.Quantity)
                .IsRequired();

            items.Property(li => li.Description)
                .IsRequired()
                .HasMaxLength(200);

            items.Property(li => li.UnitPrice)
                .HasPrecision(18, 2)
                .IsRequired();
        });
    }
}
