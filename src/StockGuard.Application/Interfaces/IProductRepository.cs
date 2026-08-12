using StockGuard.Domain.Entities;
namespace StockGuard.Application.Interfaces;
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id,CancellationToken ct);
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, Guid? categoryId, CancellationToken ct);
    Task<bool> SkuExistsAsync(string sku, CancellationToken ct);
    void Add(Product product);
    void Update(Product product);
    void Delete(Product product);
    Task<int> SaveChangesAsync(CancellationToken ct);

}