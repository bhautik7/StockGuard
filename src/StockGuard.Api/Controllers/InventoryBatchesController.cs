using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockGuard.Application.DTOs;
using StockGuard.Application.Interfaces;
using StockGuard.Domain.Entities;

namespace StockGuard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryBatchesController : ControllerBase
{
    private readonly IInventoryBatchRepository _repo;
    private readonly IStockTransactionRepository _transactionRepo;

    public InventoryBatchesController(IInventoryBatchRepository repo, IStockTransactionRepository transactionRepo)
    {
        _repo = repo;
        _transactionRepo = transactionRepo;
    }
   

    [HttpGet("by-product/{productId:guid}")]
    public async Task<ActionResult<List<InventoryBatchDto>>> GetByProduct(Guid productId, CancellationToken ct)
    {
        var batches = await _repo.GetByProductAsync(productId, ct);
        return Ok(batches.Select(ToDto));
    }

    [Authorize(Policy = "WarehouseStaff")]
[HttpPost("receive")]
public async Task<ActionResult<InventoryBatchDto>> Receive(ReceiveInventoryRequest request, CancellationToken ct)
{
    if (request.Quantity <= 0)
        return BadRequest("Quantity must be greater than zero.");

    var batch = new InventoryBatch
    {
        Id = Guid.NewGuid(),
        ProductId = request.ProductId,
        WarehouseId = request.WarehouseId,
        BatchNumber = request.BatchNumber,
        QuantityOnHand = request.Quantity,
        QuantityReserved = 0,
        ExpiryDate = request.ExpiryDate,
        Status = BatchStatus.Available
    };
    _repo.Add(batch);

    var userId = Guid.Parse(User.FindFirst("sub")!.Value); // who performed this, from the JWT
    _transactionRepo.Add(new StockTransaction
    {
        Id = Guid.NewGuid(),
        InventoryBatchId = batch.Id,
        Type = StockTransactionType.Receipt,
        QuantityChange = request.Quantity,
        PerformedByUserId = userId,
        Reference = request.BatchNumber
    });

    await _repo.SaveChangesAsync(ct); // one save — both rows commit together

    var full = await _repo.GetByIdAsync(batch.Id, ct);
    return Ok(ToDto(full!));
}

    private static InventoryBatchDto ToDto(InventoryBatch b) => new(
        b.Id, b.ProductId, b.Product.Name, b.WarehouseId, b.Warehouse.Name,
        b.BatchNumber, b.QuantityOnHand, b.QuantityReserved, b.QuantityAvailable,
        b.ExpiryDate, b.Status.ToString());
}