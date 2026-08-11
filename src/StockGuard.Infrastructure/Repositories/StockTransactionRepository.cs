using StockGuard.Application.Interfaces;
using StockGuard.Domain.Entities;
using StockGuard.Infrastructure.Persistence;

namespace StockGuard.Infrastructure.Repositories;

public class StockTransactionRepository : IStockTransactionRepository
{
    private readonly AppDbContext _context;
    public StockTransactionRepository(AppDbContext context) => _context = context;

    public void Add(StockTransaction transaction) => _context.StockTransactions.Add(transaction);
}