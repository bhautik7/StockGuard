using Microsoft.EntityFrameworkCore;
using StockGuard.Application.Interfaces;
using StockGuard.Domain.Entities;
using StockGuard.Infrastructure.Persistence;

namespace StockGuard.Infrastructure.Repositories;

public class InventoryBatchRepository : IInventoryBatchRepository
{
    private readonly AppDbContext _context;
    public InventoryBatchRepository(AppDbContext context) => _context = context;

    public Task<InventoryBatch?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _context.InventoryBatches.Include(b => b.Product).Include(b => b.Warehouse)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

    public Task<List<InventoryBatch>> GetByProductAsync(Guid productId, CancellationToken ct) =>
        _context.InventoryBatches.Include(b => b.Product).Include(b => b.Warehouse)
            .Where(b => b.ProductId == productId)
            .OrderBy(b => b.ExpiryDate) // FEFO order — soonest expiry first, ready for later
            .ToListAsync(ct);

    public void Add(InventoryBatch batch) => _context.InventoryBatches.Add(batch);
    public Task<int> SaveChangesAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);

    public Task<List<InventoryBatch>> GetAvailableForProductAsync(Guid productId, CancellationToken ct) =>
    _context.InventoryBatches
        .Where(b => b.ProductId == productId && b.Status == BatchStatus.Available)
        .OrderBy(b => b.ExpiryDate)
        .ToListAsync(ct);

    public Task<InventoryBatch?> GetByIdForUpdateAsync(Guid id, CancellationToken ct) =>
        _context.InventoryBatches.FirstOrDefaultAsync(b => b.Id == id, ct);
}