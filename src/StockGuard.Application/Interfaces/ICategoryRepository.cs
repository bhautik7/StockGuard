using StockGuard.Domain.Entities;

namespace StockGuard.Application.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync(CancellationToken ct);
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct);
    void Add(Category category);
    void Update(Category category);
    void Delete(Category category);
    Task<int> SaveChangesAsync(CancellationToken ct);
}