using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockGuard.Domain.Entities;

namespace StockGuard.Infrastructure.Persistence.Configurations;

public class InventoryBatchConfiguration : IEntityTypeConfiguration<InventoryBatch>
{
    public void Configure(EntityTypeBuilder<InventoryBatch> builder)
    {
        builder.HasKey(b => b.Id);  

        builder.Property(b => b.BatchNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(b => b.RowVersion)
            .IsRowVersion();

        builder.Ignore(b => b.QuantityAvailable); // computed in C#, not stored

        builder.HasOne(b => b.Product)
            .WithMany(p => p.InventoryBatches)
            .HasForeignKey(b => b.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Warehouse)
            .WithMany(w => w.InventoryBatches)
            .HasForeignKey(b => b.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => new { b.ProductId, b.ExpiryDate }); // speeds up FEFO queries
        builder.HasIndex(b => b.Status);
    }
}