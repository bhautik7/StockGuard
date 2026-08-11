using Microsoft.EntityFrameworkCore;
using StockGuard.Application.Interfaces;
using StockGuard.Domain.Entities;
using StockGuard.Infrastructure.Persistence;

namespace StockGuard.Infrastructure.Repositories;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly AppDbContext _context;
    public WarehouseRepository(AppDbContext context) => _context = context;

    public Task<List<Warehouse>> GetAllAsync(CancellationToken ct) =>
        _context.Warehouses.OrderBy(w => w.Name).ToListAsync(ct);

    public Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _context.Warehouses.FirstOrDefaultAsync(w => w.Id == id, ct);

    public void Add(Warehouse warehouse) => _context.Warehouses.Add(warehouse);
    public Task<int> SaveChangesAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);
}