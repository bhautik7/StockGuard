using Microsoft.AspNetCore.Mvc;
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
}