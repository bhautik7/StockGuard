namespace StockGuard.Application.DTOs;

public record ReserveInventoryRequest(Guid ProductId, int Quantity, string? IdempotencyKey);
public record ReservationLineDto(Guid InventoryBatchId, int Quantity);
public record ReservationDto(Guid Id, string Status, List<ReservationLineDto> Lines);