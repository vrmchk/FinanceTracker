using FinanceTracker.DAL.Entities.Base;

namespace FinanceTracker.DAL.Entities;

public class Account : BaseEntity
{
    public Account()
    {
        Categories = new List<Category>();
        Currencies = new List<Currency>();
    }

    public string Name { get; set; } = string.Empty;

    public int UserId { get; set; }

    public User User { get; set; } = null!;
    public List<Category> Categories { get; set; }
    public List<Currency> Currencies { get; set; }
}