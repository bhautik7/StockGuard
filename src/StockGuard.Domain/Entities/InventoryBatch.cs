namespace StockGuard.Domain.Entities;

public enum BatchStatus
{
    Available,
    Quarantined,
    Expired
}

public class InventoryBatch
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public string BatchNumber { get; set; } = string.Empty;
    public int QuantityOnHand { get; set; }
    public int QuantityReserved { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public BatchStatus Status { get; set; } = BatchStatus.Available;

    public byte[] RowVersion { get; set; } = Array.Empty<byte>(); // optimistic concurrency 

    public int QuantityAvailable => QuantityOnHand - QuantityReserved;

    public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
}