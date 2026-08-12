namespace StockGuard.Application.DTOs;

public record WarehouseDto(Guid Id, string Name, string Location);
public record CreateWarehouseRequest(string Name, string Location);
public record UpdateWarehouseRequest(string Name, string Location);