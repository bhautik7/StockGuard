namespace StockGuard.Application.DTOs;

public record TransferInventoryRequest(Guid InventoryBatchId, Guid ToWarehouseId, int Quantity);
public record AdjustInventoryRequest(Guid InventoryBatchId, int QuantityChange, string Reason);
public record QuarantineInventoryRequest(Guid InventoryBatchId, string Reason);