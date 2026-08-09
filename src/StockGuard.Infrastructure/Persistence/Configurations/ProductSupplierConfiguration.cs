using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockGuard.Domain.Entities;

namespace StockGuard.Infrastructure.Persistence.Configurations;

public class ProductSupplierConfiguration : IEntityTypeConfiguration<ProductSupplier>
{
    public void Configure(EntityTypeBuilder<ProductSupplier> builder)
    {
        builder.HasKey(ps => ps.Id);

        builder.Property(ps => ps.UnitCost)
            .HasPrecision(18, 2); // 18 total digits, 2 after the decimal point — standard for currency

        builder.HasOne(ps => ps.Product)
            .WithMany(p => p.ProductSuppliers)
            .HasForeignKey(ps => ps.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ps => ps.Supplier)
            .WithMany(s => s.ProductSuppliers)
            .HasForeignKey(ps => ps.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}