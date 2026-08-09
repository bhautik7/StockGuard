namespace StockGuard.Domain.Entities;

public class ProductSupplier
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public Guid SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;

    public decimal UnitCost { get; set; }
    public int LeadTimeDays { get; set; }
}