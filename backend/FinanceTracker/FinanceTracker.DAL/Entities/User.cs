using FinanceTracker.DAL.Entities.Base;

namespace FinanceTracker.DAL.Entities;

public class User : BaseEntity
{
    public User()
    {
        Accounts = new List<Account>();
    }

    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }

    public List<Account> Accounts { get; set; }
}