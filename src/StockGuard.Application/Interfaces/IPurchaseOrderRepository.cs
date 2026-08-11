using StockGuard.Domain.Entities;

namespace StockGuard.Application.Interfaces;

public interface IPurchaseOrderRepository
{
    Task<PurchaseOrder?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<List<PurchaseOrder>> GetAllAsync(CancellationToken ct);
    void Add(PurchaseOrder order);
    Task<int> SaveChangesAsync(CancellationToken ct);
}