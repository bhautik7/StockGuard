using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    [Authorize(Policy = "WarehouseStaff")]
    [HttpPost("transfer")]
    public async Task<ActionResult<InventoryBatchDto>> Transfer(TransferInventoryRequest request, CancellationToken ct)
    {
        if (request.Quantity <= 0)
            return BadRequest("Quantity must be greater than zero.");

        var sourceBatch = await _repo.GetByIdAsync(request.InventoryBatchId, ct);
        if (sourceBatch is null) return NotFound("Source batch not found.");

        if (sourceBatch.QuantityAvailable < request.Quantity)
            return Conflict($"Only {sourceBatch.QuantityAvailable} units available to transfer.");

        var userId = Guid.Parse(User.FindFirst("sub")!.Value);

        // Reduce the source batch
        sourceBatch.QuantityOnHand -= request.Quantity;
        _transactionRepo.Add(new StockTransaction
        {
            Id = Guid.NewGuid(),
            InventoryBatchId = sourceBatch.Id,
            Type = StockTransactionType.Transfer,
            QuantityChange = -request.Quantity,
            PerformedByUserId = userId,
            Reference = $"Transfer to warehouse {request.ToWarehouseId}"
        });

        // Create the destination batch
        var destinationBatch = new InventoryBatch
        {
            Id = Guid.NewGuid(),
            ProductId = sourceBatch.ProductId,
            WarehouseId = request.ToWarehouseId,
            BatchNumber = sourceBatch.BatchNumber,
            QuantityOnHand = request.Quantity,
            QuantityReserved = 0,
            ExpiryDate = sourceBatch.ExpiryDate,
            Status = BatchStatus.Available
        };
        _repo.Add(destinationBatch);
        _transactionRepo.Add(new StockTransaction
        {
            Id = Guid.NewGuid(),
            InventoryBatchId = destinationBatch.Id,
            Type = StockTransactionType.Transfer,
            QuantityChange = request.Quantity,
            PerformedByUserId = userId,
            Reference = $"Transfer from warehouse {sourceBatch.WarehouseId}"
        });

        try
        {
            await _repo.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("This batch was modified by someone else at the same time. Please try again.");
        }

        var full = await _repo.GetByIdAsync(destinationBatch.Id, ct);
        return Ok(ToDto(full!));
    }
    [Authorize(Policy = "InventoryManagerOrAdmin")]
    [HttpPost("adjust")]
    public async Task<ActionResult<InventoryBatchDto>> Adjust(AdjustInventoryRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest("A reason is required for any inventory adjustment.");

        var batch = await _repo.GetByIdAsync(request.InventoryBatchId, ct);
        if (batch is null) return NotFound();

        var newQuantityOnHand = batch.QuantityOnHand + request.QuantityChange;
        if (newQuantityOnHand < batch.QuantityReserved)
            return Conflict($"Cannot adjust below the {batch.QuantityReserved} units currently reserved.");

        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        batch.QuantityOnHand = newQuantityOnHand;

        _transactionRepo.Add(new StockTransaction
        {
            Id = Guid.NewGuid(),
            InventoryBatchId = batch.Id,
            Type = StockTransactionType.Adjustment,
            QuantityChange = request.QuantityChange,
            PerformedByUserId = userId,
            Reference = request.Reason
        });

        try
        {
            await _repo.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("This batch was modified by someone else at the same time. Please try again.");
        }

        var full = await _repo.GetByIdAsync(batch.Id, ct);
        return Ok(ToDto(full!));
    }

    [Authorize(Policy = "InventoryManagerOrAdmin")]
    [HttpPost("quarantine")]
    public async Task<ActionResult<InventoryBatchDto>> Quarantine(QuarantineInventoryRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest("A reason is required to quarantine a batch.");

        var batch = await _repo.GetByIdAsync(request.InventoryBatchId, ct);
        if (batch is null) return NotFound();

        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        batch.Status = BatchStatus.Quarantined;

        _transactionRepo.Add(new StockTransaction
        {
            Id = Guid.NewGuid(),
            InventoryBatchId = batch.Id,
            Type = StockTransactionType.Quarantine,
            QuantityChange = 0, // status change, not a quantity change
            PerformedByUserId = userId,
            Reference = request.Reason
        });

        await _repo.SaveChangesAsync(ct);

        var full = await _repo.GetByIdAsync(batch.Id, ct);
        return Ok(ToDto(full!));
    }

    private static InventoryBatchDto ToDto(InventoryBatch b) => new(
        b.Id, b.ProductId, b.Product.Name, b.WarehouseId, b.Warehouse.Name,
        b.BatchNumber, b.QuantityOnHand, b.QuantityReserved, b.QuantityAvailable,
        b.ExpiryDate, b.Status.ToString());
}