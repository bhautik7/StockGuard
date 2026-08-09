namespace StockGuard.Domain.Entities;

public class Warehouse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;

    public ICollection<InventoryBatch> InventoryBatches { get; set; } = new List<InventoryBatch>();
}