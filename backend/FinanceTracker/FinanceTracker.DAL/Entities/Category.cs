using FinanceTracker.DAL.Entities.Base;

namespace FinanceTracker.DAL.Entities;

public class Category : BaseEntity
{
    public Category()
    {
        SubCategories = new List<Category>();
        Transactions = new List<Transaction>();
    }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int? ParentId { get; set; }
    public int AccountId { get; set; }

    public Category? ParentCategory { get; set; }
    public List<Category> SubCategories { get; set; }
    public Account Account { get; set; } = null!;
    public List<Transaction> Transactions { get; set; }
}