using Microsoft.EntityFrameworkCore;
using StockGuard.Application.Interfaces;
using StockGuard.Domain.Entities;
using StockGuard.Infrastructure.Persistence;

namespace StockGuard.Worker;

public class ExpiryCheckWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ExpiryCheckWorker> _logger;

    public ExpiryCheckWorker(IServiceProvider services, ILogger<ExpiryCheckWorker> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckExpiringBatchesAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromMinutes(60), stoppingToken); // runs once per hour
        }
    }

    private async Task CheckExpiringBatchesAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var alertRepo = scope.ServiceProvider.GetRequiredService<IAlertRepository>();

        var soonThreshold = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

        var expiringBatches = await context.InventoryBatches
            .Where(b => b.Status == BatchStatus.Available && b.ExpiryDate <= soonThreshold && b.QuantityOnHand > 0)
            .Include(b => b.Product)
            .ToListAsync(ct);

        foreach (var batch in expiringBatches)
        {
            var alreadyAlerted = await alertRepo.ExistsUnresolvedAsync(AlertType.ExpiringSoon, batch.Id, ct);
            if (alreadyAlerted) continue; // duplicate prevention

            alertRepo.Add(new Alert
            {
                Id = Guid.NewGuid(),
                Type = AlertType.ExpiringSoon,
                Message = $"{batch.Product.Name} (batch {batch.BatchNumber}) expires on {batch.ExpiryDate}.",
                RelatedEntityId = batch.Id
            });

            _logger.LogInformation("Created expiry alert for batch {BatchId}", batch.Id);
        }

        await alertRepo.SaveChangesAsync(ct);
    }
}