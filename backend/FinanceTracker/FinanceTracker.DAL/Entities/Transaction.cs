using FinanceTracker.Common.Enums;
using FinanceTracker.DAL.Entities.Base;

namespace FinanceTracker.DAL.Entities;

public class Transaction : BaseEntity
{
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExecutionTime { get; set; }
    public string Description { get; set; } = string.Empty;

    public int AccountId { get; set; }
    public int CategoryId { get; set; }
    public int ExchangeRateId { get; set; }
    
    public Account Account { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public CurrencyExchangeRate ExchangeRate { get; set; } = null!;
}