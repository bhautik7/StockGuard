namespace StockGuard.Application.DTOs;

public record ProductDto(
    Guid Id, string Sku, string Name, string? Description,
    string Unit, int ReorderLevel, Guid CategoryId, string CategoryName);

public record CreateProductRequest(
    string Sku, string Name, string? Description,
    string Unit, int ReorderLevel, Guid CategoryId);

public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);