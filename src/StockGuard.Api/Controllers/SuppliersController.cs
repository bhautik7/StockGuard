using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockGuard.Application.DTOs;
using StockGuard.Application.Interfaces;
using StockGuard.Domain.Entities;

namespace StockGuard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierRepository _repo;
    public SuppliersController(ISupplierRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<ActionResult<List<SupplierDto>>> GetAll(CancellationToken ct)
    {
        var suppliers = await _repo.GetAllAsync(ct);
        return Ok(suppliers.Select(s => new SupplierDto(s.Id, s.Name, s.ContactEmail, s.ContactPhone)));
    }

    [HttpPost]
    public async Task<ActionResult<SupplierDto>> Create(CreateSupplierRequest request, CancellationToken ct)
    {
        var supplier = new Supplier { Id = Guid.NewGuid(), Name = request.Name, ContactEmail = request.ContactEmail, ContactPhone = request.ContactPhone };
        _repo.Add(supplier);
        await _repo.SaveChangesAsync(ct);
        var dto = new SupplierDto(supplier.Id, supplier.Name, supplier.ContactEmail, supplier.ContactPhone);
        return CreatedAtAction(nameof(GetAll), dto);
    }

    [Authorize(Policy = "PurchasingOrAdmin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SupplierDto>> Update(Guid id, UpdateSupplierRequest request, CancellationToken ct)
    {
        var supplier = await _repo.GetByIdAsync(id, ct);
        if (supplier is null) return NotFound();

        supplier.Name = request.Name;
        supplier.ContactEmail = request.ContactEmail;
        supplier.ContactPhone = request.ContactPhone;
        _repo.Update(supplier);
        await _repo.SaveChangesAsync(ct);

        return Ok(new SupplierDto(supplier.Id, supplier.Name, supplier.ContactEmail, supplier.ContactPhone));
    }

    [Authorize(Policy = "PurchasingOrAdmin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var supplier = await _repo.GetByIdAsync(id, ct);
        if (supplier is null) return NotFound();

        _repo.Delete(supplier);
        try
        {
            await _repo.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return Conflict("This supplier has products or purchase orders linked to it and cannot be deleted.");
        }

        return NoContent();
    }
}