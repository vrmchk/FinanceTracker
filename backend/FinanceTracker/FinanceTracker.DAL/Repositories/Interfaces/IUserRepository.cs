using FinanceTracker.DAL.Entities;

namespace FinanceTracker.DAL.Repositories.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, bool withRelations = true);

    Task<User?> GetByUserNameAsync(string userName, bool withRelations = true);
}