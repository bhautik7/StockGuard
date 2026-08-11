using Microsoft.EntityFrameworkCore;
using StockGuard.Application.Interfaces;
using StockGuard.Domain.Entities;
using StockGuard.Infrastructure.Persistence;

namespace StockGuard.Infrastructure.Repositories;

public class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly AppDbContext _context;
    public PurchaseOrderRepository(AppDbContext context) => _context = context;

    public Task<PurchaseOrder?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _context.PurchaseOrders.Include(po => po.Supplier).Include(po => po.Lines).ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(po => po.Id == id, ct);

    public Task<List<PurchaseOrder>> GetAllAsync(CancellationToken ct) =>
        _context.PurchaseOrders.Include(po => po.Supplier).Include(po => po.Lines).ThenInclude(l => l.Product)
            .OrderByDescending(po => po.CreatedAtUtc).ToListAsync(ct);

    public void Add(PurchaseOrder order) => _context.PurchaseOrders.Add(order);
    public Task<int> SaveChangesAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);
}