namespace StockGuard.Domain.Entities;

public enum StockTransactionType
{
    Receipt,
    Reservation,
    ReservationRelease,
    Dispatch,
    Transfer,
    Adjustment,
    Quarantine,
    Expiry
}

public class StockTransaction
{
    public Guid Id { get; set; }
    public Guid InventoryBatchId { get; set; }
    public InventoryBatch InventoryBatch { get; set; } = null!;

    public StockTransactionType Type { get; set; }
    public int QuantityChange { get; set; } // positive or negative
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    public Guid PerformedByUserId { get; set; }
    public string? Reference { get; set; } // e.g. PO number, reservation ID

    // No Update/Delete methods on purpose 
}