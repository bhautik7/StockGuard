using StockGuard.Domain.Entities;

namespace StockGuard.Application.Interfaces;

public interface IInventoryBatchRepository
{
    Task<InventoryBatch?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<List<InventoryBatch>> GetByProductAsync(Guid productId, CancellationToken ct);
    void Add(InventoryBatch batch);
    Task<int> SaveChangesAsync(CancellationToken ct);
}