using StockGuard.Domain.Entities;

namespace StockGuard.Application.Interfaces;

public interface IStockTransactionRepository
{
    void Add(StockTransaction transaction);
}