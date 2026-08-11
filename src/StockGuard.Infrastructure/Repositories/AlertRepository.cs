using Microsoft.EntityFrameworkCore;
using StockGuard.Application.Interfaces;
using StockGuard.Domain.Entities;
using StockGuard.Infrastructure.Persistence;

namespace StockGuard.Infrastructure.Repositories;

public class AlertRepository : IAlertRepository
{
    private readonly AppDbContext _context;
    public AlertRepository(AppDbContext context) => _context = context;

    public Task<bool> ExistsUnresolvedAsync(AlertType type, Guid relatedEntityId, CancellationToken ct) =>
        _context.Alerts.AnyAsync(a => a.Type == type && a.RelatedEntityId == relatedEntityId && !a.IsResolved, ct);

    public void Add(Alert alert) => _context.Alerts.Add(alert);
    public Task<int> SaveChangesAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);
}