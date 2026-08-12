using Microsoft.EntityFrameworkCore;
using StockGuard.Application.Interfaces;
using StockGuard.Domain.Entities;
using StockGuard.Infrastructure.Persistence;

namespace StockGuard.Infrastructure.Repositories;

public class SupplierRepository : ISupplierRepository
{
    private readonly AppDbContext _context;
    public SupplierRepository(AppDbContext context) => _context = context;

    public Task<List<Supplier>> GetAllAsync(CancellationToken ct) =>
        _context.Suppliers.OrderBy(s => s.Name).ToListAsync(ct);

    public Task<Supplier?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _context.Suppliers.FirstOrDefaultAsync(s => s.Id == id, ct);

    public void Add(Supplier supplier) => _context.Suppliers.Add(supplier);
    public void Update(Supplier supplier) => _context.Suppliers.Update(supplier);
    public void Delete(Supplier supplier) => _context.Suppliers.Remove(supplier);
    public Task<int> SaveChangesAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);
}