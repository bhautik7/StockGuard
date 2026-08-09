using Microsoft.EntityFrameworkCore;
using StockGuard.Domain.Entities;
using StockGuard.Infrastructure.Persistence;
using StockGuard.Infrastructure.Repositories;
using Xunit;

namespace StockGuard.IntegrationTests;

public class ProductRepositoryTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // fresh isolated DB per test
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task SkuExistsAsync_ReturnsTrue_WhenProductWithSkuAlreadySaved()
    {
        // Arrange
        await using var context = CreateContext();
        var repo = new ProductRepository(context);
        var category = new Category { Id = Guid.NewGuid(), Name = "Dairy" };
        var product = new Product
        {
            Id = Guid.NewGuid(), Sku = "MILK-001", Name = "Whole Milk",
            Unit = "each", ReorderLevel = 10, CategoryId = category.Id, Category = category
        };
        context.Categories.Add(category);
        repo.Add(product);
        await repo.SaveChangesAsync(CancellationToken.None);

        // Act
        var exists = await repo.SkuExistsAsync("MILK-001", CancellationToken.None);

        // Assert
        Assert.True(exists);
    }
}