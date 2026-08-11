namespace StockGuard.Application.DTOs;

public record InventoryBatchDto(
    Guid Id, Guid ProductId, string ProductName, Guid WarehouseId, string WarehouseName,
    string BatchNumber, int QuantityOnHand, int QuantityReserved, int QuantityAvailable,
    DateOnly ExpiryDate, string Status);

public record ReceiveInventoryRequest(
    Guid ProductId, Guid WarehouseId, string BatchNumber, int Quantity, DateOnly ExpiryDate);