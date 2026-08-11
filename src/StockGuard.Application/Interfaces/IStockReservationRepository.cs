using StockGuard.Domain.Entities;

namespace StockGuard.Application.Interfaces;

public interface IStockReservationRepository
{
    Task<StockReservation?> GetByIdempotencyKeyAsync(string key, CancellationToken ct);
    void Add(StockReservation reservation);
    Task<int> SaveChangesAsync(CancellationToken ct);
}