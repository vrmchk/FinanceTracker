using FinanceTracker.DAL.Entities.Base;

namespace FinanceTracker.DAL.Entities;

public class CurrencyExchangeRate : BaseEntity
{
    public decimal ExchangeRate { get; set; }
    public DateTime UpdateTime { get; set; }
    
    public int CurrencyId { get; set; }
    public Currency Currency { get; set; } = null!;
}