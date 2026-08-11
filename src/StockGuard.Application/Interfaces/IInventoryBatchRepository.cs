using StockGuard.Domain.Entities;

namespace StockGuard.Application.Interfaces;

public interface IInventoryBatchRepository
{
    Task<InventoryBatch?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<List<InventoryBatch>> GetByProductAsync(Guid productId, CancellationToken ct);
    void Add(InventoryBatch batch);
    Task<int> SaveChangesAsync(CancellationToken ct);
    Task<List<InventoryBatch>> GetAvailableForProductAsync(Guid productId, CancellationToken ct);
    Task<InventoryBatch?> GetByIdForUpdateAsync(Guid id, CancellationToken ct);
}