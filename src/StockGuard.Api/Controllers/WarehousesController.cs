using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    [Authorize(Policy = "InventoryManagerOrAdmin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<WarehouseDto>> Update(Guid id, UpdateWarehouseRequest request, CancellationToken ct)
    {
        var warehouse = await _repo.GetByIdAsync(id, ct);
        if (warehouse is null) return NotFound();

        warehouse.Name = request.Name;
        warehouse.Location = request.Location;
        _repo.Update(warehouse);
        await _repo.SaveChangesAsync(ct);

        return Ok(new WarehouseDto(warehouse.Id, warehouse.Name, warehouse.Location));
    }

    [Authorize(Policy = "InventoryManagerOrAdmin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var warehouse = await _repo.GetByIdAsync(id, ct);
        if (warehouse is null) return NotFound();

        _repo.Delete(warehouse);
        try
        {
            await _repo.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return Conflict("This warehouse still has inventory batches and cannot be deleted.");
        }

        return NoContent();
    }
}