using StockGuard.Domain.Entities;

namespace StockGuard.Application.Interfaces;

public interface ISupplierRepository
{
    Task<List<Supplier>> GetAllAsync(CancellationToken ct);
    Task<Supplier?> GetByIdAsync(Guid id, CancellationToken ct);
    void Add(Supplier supplier);
    void Update(Supplier supplier);
    void Delete(Supplier supplier);
    Task<int> SaveChangesAsync(CancellationToken ct);
}