namespace StockGuard.Application.DTOs;

public record SupplierDto(Guid Id, string Name, string? ContactEmail, string? ContactPhone);
public record CreateSupplierRequest(string Name, string? ContactEmail, string? ContactPhone);