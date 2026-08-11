namespace StockGuard.Application.DTOs;

public record PurchaseOrderLineDto(Guid ProductId, string ProductName, int QuantityOrdered, int QuantityReceived, DateOnly ExpectedDeliveryDate);
public record PurchaseOrderDto(Guid Id, string OrderNumber, Guid SupplierId, string SupplierName, string Status, List<PurchaseOrderLineDto> Lines);

public record CreatePurchaseOrderLineRequest(Guid ProductId, int QuantityOrdered, DateOnly ExpectedDeliveryDate);
public record CreatePurchaseOrderRequest(Guid SupplierId, List<CreatePurchaseOrderLineRequest> Lines);