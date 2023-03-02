using FinanceTracker.DAL.Entities.Base;

namespace FinanceTracker.DAL.Repositories.Interfaces;

public interface IRepository<T> where T : BaseEntity, new()
{
    IAsyncEnumerable<T> GetAllAsync(bool withRelations = true);

    IAsyncEnumerable<T> GetAllAsync(Func<T, bool> predicate, bool withRelations = true);

    Task<T?> GetByIdAsync(int id, bool withRelations = true);

    Task<T?> GetAsync(Func<T, bool> predicate, bool withRelations = true);

    Task<int> AddAsync(T entity);

    Task<int> AddRangeAsync(List<T> entities);

    Task<int> UpdateAsync(T entity);

    Task<int> DeleteAsync(T entity);
}