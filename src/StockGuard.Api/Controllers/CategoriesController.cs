using Microsoft.AspNetCore.Mvc;
using StockGuard.Application.DTOs;
using StockGuard.Application.Interfaces;
using StockGuard.Domain.Entities;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
namespace StockGuard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _repo;
    private readonly IDistributedCache _cache;

    public CategoriesController(ICategoryRepository repo, IDistributedCache cache)
    {
        _repo = repo;
        _cache = cache;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll(CancellationToken ct)
    {
        const string cacheKey = "categories:all";

        var cached = await _cache.GetStringAsync(cacheKey, ct);
        if (cached is not null)
        {
            var cachedDtos = JsonSerializer.Deserialize<List<CategoryDto>>(cached);
            return Ok(cachedDtos);
        }

        var categories = await _repo.GetAllAsync(ct);
        var dtos = categories.Select(c => new CategoryDto(c.Id, c.Name, c.Description)).ToList();

        var serialized = JsonSerializer.Serialize(dtos);
        await _cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        }, ct);

        return Ok(dtos);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryRequest request, CancellationToken ct)
    {
        var category = new Category { Id = Guid.NewGuid(), Name = request.Name, Description = request.Description };
        _repo.Add(category);
        await _repo.SaveChangesAsync(ct);

        await _cache.RemoveAsync("categories:all", ct); // cache invalidation

        var dto = new CategoryDto(category.Id, category.Name, category.Description);
        return CreatedAtAction(nameof(GetAll), dto);
    }
}