using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockGuard.Application.DTOs;
using StockGuard.Application.Interfaces;
using StockGuard.Domain.Entities;

namespace StockGuard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IPurchaseOrderRepository _repo;
    public PurchaseOrdersController(IPurchaseOrderRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<ActionResult<List<PurchaseOrderDto>>> GetAll(CancellationToken ct)
    {
        var orders = await _repo.GetAllAsync(ct);
        return Ok(orders.Select(ToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PurchaseOrderDto>> GetById(Guid id, CancellationToken ct)
    {
        var order = await _repo.GetByIdAsync(id, ct);
        if (order is null) return NotFound();
        return Ok(ToDto(order));
    }

    [Authorize(Policy = "PurchasingOrAdmin")]
    [HttpPost]
    public async Task<ActionResult<PurchaseOrderDto>> Create(CreatePurchaseOrderRequest request, CancellationToken ct)
    {
        if (request.Lines.Count == 0)
            return BadRequest("A purchase order must have at least one line.");

        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        var order = new PurchaseOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = $"PO-{DateTime.UtcNow:yyyyMMddHHmmss}",
            SupplierId = request.SupplierId,
            Status = PurchaseOrderStatus.Draft,
            CreatedByUserId = userId,
            Lines = request.Lines.Select(l => new PurchaseOrderLine
            {
                Id = Guid.NewGuid(),
                ProductId = l.ProductId,
                QuantityOrdered = l.QuantityOrdered,
                QuantityReceived = 0,
                ExpectedDeliveryDate = l.ExpectedDeliveryDate
            }).ToList()
        };
        _repo.Add(order);
        await _repo.SaveChangesAsync(ct);

        var full = await _repo.GetByIdAsync(order.Id, ct);
        return Ok(ToDto(full!));
    }
    [Authorize(Policy = "PurchasingOrAdmin")]
[HttpPost("{id:guid}/submit")]
public async Task<ActionResult<PurchaseOrderDto>> Submit(Guid id, CancellationToken ct)
{
    var order = await _repo.GetByIdAsync(id, ct);
    if (order is null) return NotFound();
    if (order.Status != PurchaseOrderStatus.Draft)
        return BadRequest($"Cannot submit an order in status '{order.Status}'.");

    order.Status = PurchaseOrderStatus.Submitted;
    await _repo.SaveChangesAsync(ct);
    return Ok(ToDto(order));
}

[Authorize(Policy = "InventoryManagerOrAdmin")]
[HttpPost("{id:guid}/approve")]
public async Task<ActionResult<PurchaseOrderDto>> Approve(Guid id, CancellationToken ct)
{
    var order = await _repo.GetByIdAsync(id, ct);
    if (order is null) return NotFound();
    if (order.Status != PurchaseOrderStatus.Submitted)
        return BadRequest($"Cannot approve an order in status '{order.Status}'.");

    var userId = Guid.Parse(User.FindFirst("sub")!.Value);
    if (order.CreatedByUserId == userId)
        return BadRequest("You cannot approve your own purchase order."); // segregation of duties, from Phase 1

    order.Status = PurchaseOrderStatus.Approved;
    order.ApprovedByUserId = userId;
    await _repo.SaveChangesAsync(ct);
    return Ok(ToDto(order));
}

[Authorize(Policy = "PurchasingOrAdmin")]
[HttpPost("{id:guid}/cancel")]
public async Task<ActionResult<PurchaseOrderDto>> Cancel(Guid id, CancellationToken ct)
{
    var order = await _repo.GetByIdAsync(id, ct);
    if (order is null) return NotFound();
    if (order.Status is PurchaseOrderStatus.Received or PurchaseOrderStatus.PartiallyReceived)
        return BadRequest("Cannot cancel an order that has already received stock.");

    order.Status = PurchaseOrderStatus.Cancelled;
    await _repo.SaveChangesAsync(ct);
    return Ok(ToDto(order));
}
    private static PurchaseOrderDto ToDto(PurchaseOrder po) => new(
        po.Id, po.OrderNumber, po.SupplierId, po.Supplier.Name, po.Status.ToString(),
        po.Lines.Select(l => new PurchaseOrderLineDto(l.ProductId, l.Product.Name, l.QuantityOrdered, l.QuantityReceived, l.ExpectedDeliveryDate)).ToList());
}