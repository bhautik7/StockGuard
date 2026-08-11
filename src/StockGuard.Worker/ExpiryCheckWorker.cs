using Microsoft.EntityFrameworkCore;
using StockGuard.Application.Interfaces;
using StockGuard.Domain.Entities;
using StockGuard.Infrastructure.Persistence;

namespace StockGuard.Worker;

public class ExpiryCheckWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ExpiryCheckWorker> _logger;
    private readonly AlertBroadcaster _broadcaster;

    public ExpiryCheckWorker(IServiceProvider services, ILogger<ExpiryCheckWorker> logger, AlertBroadcaster broadcaster)
    {
        _services = services;
        _logger = logger;
        _broadcaster = broadcaster;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckExpiringBatchesAsync(stoppingToken);
            await CheckLowStockAsync(stoppingToken);
            await CheckQuarantinedBatchesAsync(stoppingToken);
            await CheckDelayedPurchaseOrdersAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromMinutes(60), stoppingToken);
        }
    }

    private async Task CheckLowStockAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var alertRepo = scope.ServiceProvider.GetRequiredService<IAlertRepository>();

        var products = await context.Products.ToListAsync(ct);

        foreach (var product in products)
        {
            var totalAvailable = await context.InventoryBatches
                .Where(b => b.ProductId == product.Id && b.Status == BatchStatus.Available)
                .SumAsync(b => b.QuantityOnHand - b.QuantityReserved, ct);

            if (totalAvailable > product.ReorderLevel) continue;

            var alreadyAlerted = await alertRepo.ExistsUnresolvedAsync(AlertType.BelowReorderLevel, product.Id, ct);
            if (alreadyAlerted) continue;

            alertRepo.Add(new Alert
            {
                Id = Guid.NewGuid(),
                Type = AlertType.BelowReorderLevel,
                Message = $"{product.Name} is at {totalAvailable} units, at or below its reorder level of {product.ReorderLevel}.",
                RelatedEntityId = product.Id
            });
            _logger.LogInformation("Created low-stock alert for product {ProductId}", product.Id);
            await _broadcaster.BroadcastAlertAsync($"New alert: {product.Name} is below reorder level.");
        }

        await alertRepo.SaveChangesAsync(ct);
    }

    private async Task CheckQuarantinedBatchesAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var alertRepo = scope.ServiceProvider.GetRequiredService<IAlertRepository>();

        var quarantinedBatches = await context.InventoryBatches
            .Where(b => b.Status == BatchStatus.Quarantined)
            .Include(b => b.Product)
            .ToListAsync(ct);

        foreach (var batch in quarantinedBatches)
        {
            var alreadyAlerted = await alertRepo.ExistsUnresolvedAsync(AlertType.QuarantinedBatch, batch.Id, ct);
            if (alreadyAlerted) continue;

            alertRepo.Add(new Alert
            {
                Id = Guid.NewGuid(),
                Type = AlertType.QuarantinedBatch,
                Message = $"{batch.Product.Name} (batch {batch.BatchNumber}) is quarantined and needs review.",
                RelatedEntityId = batch.Id
            });
            _logger.LogInformation("Created quarantine alert for batch {BatchId}", batch.Id);
            await _broadcaster.BroadcastAlertAsync($"New alert: {batch.Product.Name} (batch {batch.BatchNumber}) is quarantined.");
        }

        await alertRepo.SaveChangesAsync(ct);
    }

    private async Task CheckDelayedPurchaseOrdersAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var alertRepo = scope.ServiceProvider.GetRequiredService<IAlertRepository>();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var delayedOrders = await context.PurchaseOrders
            .Where(po => po.Status == PurchaseOrderStatus.Approved || po.Status == PurchaseOrderStatus.PartiallyReceived)
            .Include(po => po.Lines)
            .Where(po => po.Lines.Any(l => l.ExpectedDeliveryDate < today && l.QuantityReceived < l.QuantityOrdered))
            .ToListAsync(ct);

        foreach (var order in delayedOrders)
        {
            var alreadyAlerted = await alertRepo.ExistsUnresolvedAsync(AlertType.DelayedPurchaseOrder, order.Id, ct);
            if (alreadyAlerted) continue;

            alertRepo.Add(new Alert
            {
                Id = Guid.NewGuid(),
                Type = AlertType.DelayedPurchaseOrder,
                Message = $"Purchase order {order.OrderNumber} has overdue, unreceived lines.",
                RelatedEntityId = order.Id
            });
            _logger.LogInformation("Created delayed-PO alert for order {OrderId}", order.Id);
            await _broadcaster.BroadcastAlertAsync($"New alert: Purchase order {order.OrderNumber} is delayed.");
        }

        await alertRepo.SaveChangesAsync(ct);
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
            if (alreadyAlerted) continue;

            alertRepo.Add(new Alert
            {
                Id = Guid.NewGuid(),
                Type = AlertType.ExpiringSoon,
                Message = $"{batch.Product.Name} (batch {batch.BatchNumber}) expires on {batch.ExpiryDate}.",
                RelatedEntityId = batch.Id
            });

            _logger.LogInformation("Created expiry alert for batch {BatchId}", batch.Id);
            await _broadcaster.BroadcastAlertAsync($"New alert: {batch.Product.Name} expires soon.");
        }

        await alertRepo.SaveChangesAsync(ct);
    }
}