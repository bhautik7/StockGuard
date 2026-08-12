namespace StockGuard.Application.DTOs;

public record SupplierDto(Guid Id, string Name, string? ContactEmail, string? ContactPhone);
public record CreateSupplierRequest(string Name, string? ContactEmail, string? ContactPhone);
public record UpdateSupplierRequest(string Name, string? ContactEmail, string? ContactPhone);
public record ReceivePurchaseOrderLineRequest(Guid ProductId, int QuantityReceived, string BatchNumber, DateOnly ExpiryDate, Guid WarehouseId);
public record ReceivePurchaseOrderRequest(List<ReceivePurchaseOrderLineRequest> Lines);