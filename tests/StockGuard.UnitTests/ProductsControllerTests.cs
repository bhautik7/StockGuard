using Moq;
using StockGuard.Api.Controllers;
using StockGuard.Application.DTOs;
using StockGuard.Application.Interfaces;
using StockGuard.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace StockGuard.UnitTests;

public class ProductsControllerTests
{
    [Fact]
    public async Task Create_ReturnsConflict_WhenSkuAlreadyExists()
    {
        // Arrange
        var productRepo = new Mock<IProductRepository>();
        var categoryRepo = new Mock<ICategoryRepository>();
        productRepo.Setup(r => r.SkuExistsAsync("MILK-001", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

        var controller = new ProductsController(productRepo.Object, categoryRepo.Object);
        var request = new CreateProductRequest("MILK-001", "Whole Milk", null, "each", 10, Guid.NewGuid());

        // Act
        var result = await controller.Create(request, CancellationToken.None);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal(409, conflictResult.StatusCode);
    }
}