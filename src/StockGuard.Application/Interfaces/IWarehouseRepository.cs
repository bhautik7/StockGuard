using StockGuard.Domain.Entities;

namespace StockGuard.Application.Interfaces;

public interface IWarehouseRepository
{
    Task<List<Warehouse>> GetAllAsync(CancellationToken ct);
    Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken ct);
    void Add(Warehouse warehouse);
    Task<int> SaveChangesAsync(CancellationToken ct);
}