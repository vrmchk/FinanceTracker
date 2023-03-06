using FinanceTracker.DAL.Entities.Base;

namespace FinanceTracker.DAL.Entities;

public class Currency : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
}