using Microsoft.EntityFrameworkCore;
using StockGuard.Application.Interfaces;
using StockGuard.Domain.Entities;
using StockGuard.Infrastructure.Persistence;

namespace StockGuard.Infrastructure.Repositories;

public class StockReservationRepository : IStockReservationRepository
{
    private readonly AppDbContext _context;
    public StockReservationRepository(AppDbContext context) => _context = context;

    public Task<StockReservation?> GetByIdempotencyKeyAsync(string key, CancellationToken ct) =>
        _context.StockReservations.Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.IdempotencyKey == key, ct);

    public void Add(StockReservation reservation) => _context.StockReservations.Add(reservation);
    public Task<int> SaveChangesAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);
}