namespace StockGuard.Domain.Entities;

public enum ReservationStatus
{
    Active,
    Released,
    Dispatched
}

public class StockReservation
{
    public Guid Id { get; set; }
    public Guid ReservedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ReservationStatus Status { get; set; } = ReservationStatus.Active;
    public string? IdempotencyKey { get; set; } // duplicate-request protection

    public ICollection<StockReservationLine> Lines { get; set; } = new List<StockReservationLine>();
}

public class StockReservationLine
{
    public Guid Id { get; set; }
    public Guid StockReservationId { get; set; }
    public StockReservation StockReservation { get; set; } = null!;
    public Guid InventoryBatchId { get; set; }
    public InventoryBatch InventoryBatch { get; set; } = null!;
    public int Quantity { get; set; }
}