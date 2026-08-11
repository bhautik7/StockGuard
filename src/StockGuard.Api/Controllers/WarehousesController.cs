using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockGuard.Application.DTOs;
using StockGuard.Application.Interfaces;
using StockGuard.Domain.Entities;

namespace StockGuard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehousesController : ControllerBase
{
    private readonly IWarehouseRepository _repo;
    public WarehousesController(IWarehouseRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<ActionResult<List<WarehouseDto>>> GetAll(CancellationToken ct)
    {
        var warehouses = await _repo.GetAllAsync(ct);
        return Ok(warehouses.Select(w => new WarehouseDto(w.Id, w.Name, w.Location)));
    }

    [Authorize(Policy = "InventoryManagerOrAdmin")]
    [HttpPost]
    public async Task<ActionResult<WarehouseDto>> Create(CreateWarehouseRequest request, CancellationToken ct)
    {
        var warehouse = new Warehouse { Id = Guid.NewGuid(), Name = request.Name, Location = request.Location };
        _repo.Add(warehouse);
        await _repo.SaveChangesAsync(ct);
        return Ok(new WarehouseDto(warehouse.Id, warehouse.Name, warehouse.Location));
    }
}