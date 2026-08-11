using StockGuard.Domain.Entities;

namespace StockGuard.Application.Services;

public class InsufficientStockException : Exception
{
    public InsufficientStockException(string message) : base(message) { }
}

public record BatchAllocation(Guid BatchId, int Quantity);

public class FefoAllocationService
{
    public List<BatchAllocation> Allocate(List<InventoryBatch> availableBatches, int quantityNeeded)
    {
        var eligibleBatches = availableBatches
            .Where(b => b.Status == BatchStatus.Available && b.QuantityAvailable > 0)
            .OrderBy(b => b.ExpiryDate)
            .ToList();

        var allocations = new List<BatchAllocation>();
        var remaining = quantityNeeded;

        foreach (var batch in eligibleBatches)
        {
            if (remaining <= 0) break;

            var takeFromThisBatch = Math.Min(remaining, batch.QuantityAvailable);
            allocations.Add(new BatchAllocation(batch.Id, takeFromThisBatch));
            remaining -= takeFromThisBatch;
        }

        if (remaining > 0)
        {
            var totalAvailable = eligibleBatches.Sum(b => b.QuantityAvailable);
            throw new InsufficientStockException(
                $"Requested {quantityNeeded} units, but only {totalAvailable} available across eligible batches.");
        }

        return allocations;
    }
}