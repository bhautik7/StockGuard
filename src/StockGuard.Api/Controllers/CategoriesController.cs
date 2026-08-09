using Microsoft.AspNetCore.Mvc;
using StockGuard.Application.DTOs;
using StockGuard.Application.Interfaces;
using StockGuard.Domain.Entities;

namespace StockGuard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _repo;
    public CategoriesController(ICategoryRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll(CancellationToken ct)
    {
        var categories = await _repo.GetAllAsync(ct);
        return Ok(categories.Select(c => new CategoryDto(c.Id, c.Name, c.Description)));
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryRequest request, CancellationToken ct)
    {
        var category = new Category { Id = Guid.NewGuid(), Name = request.Name, Description = request.Description };
        _repo.Add(category);
        await _repo.SaveChangesAsync(ct);
        var dto = new CategoryDto(category.Id, category.Name, category.Description);
        return CreatedAtAction(nameof(GetAll), dto);
    }
}