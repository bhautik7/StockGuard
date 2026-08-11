using Microsoft.EntityFrameworkCore;
using StockGuard.Infrastructure.Persistence;

namespace StockGuard.Worker;

public class OutboxPublisherWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<OutboxPublisherWorker> _logger;

    public OutboxPublisherWorker(IServiceProvider services, ILogger<OutboxPublisherWorker> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await PublishPendingMessagesAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
    }

    private async Task PublishPendingMessagesAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var pending = await context.OutboxMessages
            .Where(m => m.ProcessedAtUtc == null)
            .OrderBy(m => m.CreatedAtUtc)
            .Take(20)
            .ToListAsync(ct);

        foreach (var message in pending)
        {
            try
            {
                // In production, this is where we'd call Azure Service Bus's SendMessageAsync.
                // For now, we simulate that by logging — the pattern is identical either way.
                _logger.LogInformation("Publishing outbox message {Id} of type {Type}: {Payload}",
                    message.Id, message.Type, message.Payload);

                message.ProcessedAtUtc = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                _logger.LogWarning(ex, "Failed to publish outbox message {Id}, retry count now {Count}",
                    message.Id, message.RetryCount);
            }
        }

        await context.SaveChangesAsync(ct);
    }
}