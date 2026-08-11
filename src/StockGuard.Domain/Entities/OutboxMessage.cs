namespace StockGuard.Domain.Entities;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty; // e.g. "InventoryReceived"
    public string Payload { get; set; } = string.Empty; // JSON content of the event
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAtUtc { get; set; } // null = not sent yet
    public int RetryCount { get; set; }
}