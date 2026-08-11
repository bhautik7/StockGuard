using StockGuard.Domain.Entities;

namespace StockGuard.Application.Interfaces;

public interface IAlertRepository
{
    Task<bool> ExistsUnresolvedAsync(AlertType type, Guid relatedEntityId, CancellationToken ct);
    void Add(Alert alert);
    Task<int> SaveChangesAsync(CancellationToken ct);
}