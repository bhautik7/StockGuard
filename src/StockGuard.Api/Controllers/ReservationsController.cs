using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockGuard.Application.DTOs;
using StockGuard.Application.Interfaces;
using StockGuard.Application.Services;
using StockGuard.Domain.Entities;

namespace StockGuard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "WarehouseStaff")]
public class ReservationsController : ControllerBase
{
    private readonly IInventoryBatchRepository _batchRepo;
    private readonly IStockReservationRepository _reservationRepo;
    private readonly IStockTransactionRepository _transactionRepo;
    private readonly FefoAllocationService _fefoService;

    public ReservationsController(
        IInventoryBatchRepository batchRepo,
        IStockReservationRepository reservationRepo,
        IStockTransactionRepository transactionRepo,
        FefoAllocationService fefoService)
    {
        _batchRepo = batchRepo;
        _reservationRepo = reservationRepo;
        _transactionRepo = transactionRepo;
        _fefoService = fefoService;
    }

    [HttpPost]
    public async Task<ActionResult<ReservationDto>> Reserve(ReserveInventoryRequest request, CancellationToken ct)
    {
        if (request.Quantity <= 0)
            return BadRequest("Quantity must be greater than zero.");
        
            // Idempotency check — if we've already processed this exact request, return the original result
        if (!string.IsNullOrEmpty(request.IdempotencyKey))
        {
            var existing = await _reservationRepo.GetByIdempotencyKeyAsync(request.IdempotencyKey, ct);
            if (existing is not null)
            {
                return Ok(new ReservationDto(
                    existing.Id, existing.Status.ToString(),
                    existing.Lines.Select(l => new ReservationLineDto(l.InventoryBatchId, l.Quantity)).ToList()));
            }
        }

        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        var availableBatches = await _batchRepo.GetAvailableForProductAsync(request.ProductId, ct);

        List<BatchAllocation> allocations;
        try
        {
            allocations = _fefoService.Allocate(availableBatches, request.Quantity);
        }
        catch (InsufficientStockException ex)
        {
            return Conflict(ex.Message);
        }

        var reservation = new StockReservation
        {
            Id = Guid.NewGuid(),
            ReservedByUserId = userId,
            Status = ReservationStatus.Active,
            IdempotencyKey = request.IdempotencyKey
        };

        foreach (var allocation in allocations)
        {
            var batch = await _batchRepo.GetByIdForUpdateAsync(allocation.BatchId, ct);
            batch!.QuantityReserved += allocation.Quantity;

            reservation.Lines.Add(new StockReservationLine
            {
                Id = Guid.NewGuid(),
                InventoryBatchId = batch.Id,
                Quantity = allocation.Quantity
            });

            _transactionRepo.Add(new StockTransaction
            {
                Id = Guid.NewGuid(),
                InventoryBatchId = batch.Id,
                Type = StockTransactionType.Reservation,
                QuantityChange = -allocation.Quantity, // negative — reserving reduces what's freely available
                PerformedByUserId = userId,
                Reference = reservation.Id.ToString()
            });
        }

        _reservationRepo.Add(reservation);
        try
        {
            await _reservationRepo.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("The requested stock was reserved by someone else at the same time. Please try again.");
        }

        return Ok(new ReservationDto(
            reservation.Id, reservation.Status.ToString(),
            reservation.Lines.Select(l => new ReservationLineDto(l.InventoryBatchId, l.Quantity)).ToList()));
        }
}