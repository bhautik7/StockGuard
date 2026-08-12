using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockGuard.Application.DTOs;
using StockGuard.Application.Interfaces;
using StockGuard.Domain.Entities;

namespace StockGuard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repo;
    private readonly ICategoryRepository _categoryRepo;
    public ProductsController(IProductRepository repo, ICategoryRepository categoryRepo)
    {
        _repo = repo;
        _categoryRepo = categoryRepo;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetPaged(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null, [FromQuery] Guid? categoryId = null,
        CancellationToken ct = default)
    {
        var (items, total) = await _repo.GetPagedAsync(page, pageSize, search, categoryId, ct);
        var dtos = items.Select(p => new ProductDto(p.Id, p.Sku, p.Name, p.Description, p.Unit, p.ReorderLevel, p.CategoryId, p.Category.Name)).ToList();
        return Ok(new PagedResult<ProductDto>(dtos, total, page, pageSize));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken ct)
    {
        var p = await _repo.GetByIdAsync(id, ct);
        if (p is null) return NotFound();
        return Ok(new ProductDto(p.Id, p.Sku, p.Name, p.Description, p.Unit, p.ReorderLevel, p.CategoryId, p.Category.Name));
    }
    [Authorize(Policy = "InventoryManagerOrAdmin")]
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request, CancellationToken ct)
    {
        if (await _repo.SkuExistsAsync(request.Sku, ct))
            return Conflict($"SKU '{request.Sku}' already exists.");

        var category = await _categoryRepo.GetByIdAsync(request.CategoryId, ct);
        if (category is null) return BadRequest("Invalid CategoryId.");

        var product = new Product
        {
            Id = Guid.NewGuid(), Sku = request.Sku, Name = request.Name, Description = request.Description,
            Unit = request.Unit, ReorderLevel = request.ReorderLevel, CategoryId = request.CategoryId
        };
        _repo.Add(product);
        await _repo.SaveChangesAsync(ct);

        var dto = new ProductDto(product.Id, product.Sku, product.Name, product.Description, product.Unit, product.ReorderLevel, product.CategoryId, category.Name);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, dto);
    }

    [Authorize(Policy = "InventoryManagerOrAdmin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDto>> Update(Guid id, UpdateProductRequest request, CancellationToken ct)
    {
        var product = await _repo.GetByIdAsync(id, ct);
        if (product is null) return NotFound();

        if (!string.Equals(product.Sku, request.Sku, StringComparison.Ordinal) && await _repo.SkuExistsAsync(request.Sku, ct))
            return Conflict($"SKU '{request.Sku}' already exists.");

        var category = await _categoryRepo.GetByIdAsync(request.CategoryId, ct);
        if (category is null) return BadRequest("Invalid CategoryId.");

        product.Sku = request.Sku;
        product.Name = request.Name;
        product.Description = request.Description;
        product.Unit = request.Unit;
        product.ReorderLevel = request.ReorderLevel;
        product.CategoryId = request.CategoryId;

        _repo.Update(product);
        await _repo.SaveChangesAsync(ct);

        var dto = new ProductDto(product.Id, product.Sku, product.Name, product.Description, product.Unit, product.ReorderLevel, product.CategoryId, category.Name);
        return Ok(dto);
    }

    [Authorize(Policy = "InventoryManagerOrAdmin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var product = await _repo.GetByIdAsync(id, ct);
        if (product is null) return NotFound();

        _repo.Delete(product);
        try
        {
            await _repo.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return Conflict("This product has inventory batches or reservations and cannot be deleted.");
        }

        return NoContent();
    }
}