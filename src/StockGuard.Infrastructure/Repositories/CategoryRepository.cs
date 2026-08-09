using Microsoft.EntityFrameworkCore;
using StockGuard.Application.Interfaces;
using StockGuard.Domain.Entities;
using StockGuard.Infrastructure.Persistence;

namespace StockGuard.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;
    public CategoryRepository(AppDbContext context) => _context = context;

    public Task<List<Category>> GetAllAsync(CancellationToken ct) =>
        _context.Categories.OrderBy(c => c.Name).ToListAsync(ct);

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _context.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);

    public void Add(Category category) => _context.Categories.Add(category);
    public Task<int> SaveChangesAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);
}