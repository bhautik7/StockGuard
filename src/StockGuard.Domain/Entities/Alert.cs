namespace StockGuard.Domain.Entities;

public enum AlertType
{
    ExpiringSoon,
    BelowReorderLevel,
    QuarantinedBatch,
    DelayedPurchaseOrder
}

public class Alert
{
    public Guid Id { get; set; }
    public AlertType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? RelatedEntityId { get; set; } // e.g. the InventoryBatch or PurchaseOrder this is about
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsResolved { get; set; }
}