using StockGuard.Application.Services;
using StockGuard.Domain.Entities;
using Xunit;

namespace StockGuard.UnitTests;

public class FefoAllocationServiceTests
{
    private static InventoryBatch MakeBatch(int quantity, DateOnly expiry) => new()
    {
        Id = Guid.NewGuid(),
        QuantityOnHand = quantity,
        QuantityReserved = 0,
        ExpiryDate = expiry,
        Status = BatchStatus.Available
    };

    [Fact]
    public void Allocate_TakesFromEarliestExpiringBatchesFirst()
    {
        // Arrange — exactly the example from Phase 8 Step 1
        var batchA = MakeBatch(20, new DateOnly(2026, 9, 10));
        var batchB = MakeBatch(50, new DateOnly(2026, 9, 20));
        var batchC = MakeBatch(40, new DateOnly(2026, 10, 5));
        var batches = new List<InventoryBatch> { batchC, batchA, batchB }; // deliberately out of order

        var service = new FefoAllocationService();

        // Act
        var result = service.Allocate(batches, 30);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(batchA.Id, result[0].BatchId);
        Assert.Equal(20, result[0].Quantity);
        Assert.Equal(batchB.Id, result[1].BatchId);
        Assert.Equal(10, result[1].Quantity);
    }

    [Fact]
    public void Allocate_ThrowsWhenNotEnoughStock()
    {
        var batch = MakeBatch(10, new DateOnly(2026, 9, 10));
        var service = new FefoAllocationService();

        Assert.Throws<InsufficientStockException>(() => service.Allocate(new List<InventoryBatch> { batch }, 50));
    }

    [Fact]
    public void Allocate_SkipsQuarantinedBatches()
    {
        var quarantined = MakeBatch(100, new DateOnly(2026, 9, 1));
        quarantined.Status = BatchStatus.Quarantined;
        var available = MakeBatch(30, new DateOnly(2026, 9, 15));

        var service = new FefoAllocationService();
        var result = service.Allocate(new List<InventoryBatch> { quarantined, available }, 30);

        Assert.Single(result);
        Assert.Equal(available.Id, result[0].BatchId);
    }
}