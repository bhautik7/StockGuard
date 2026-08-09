namespace StockGuard.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public Guid PerformedByUserId { get; set; }
    public string Action { get; set; } = string.Empty; // e.g. "PurchaseOrder.Approved"
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    public string? Details { get; set; }
}