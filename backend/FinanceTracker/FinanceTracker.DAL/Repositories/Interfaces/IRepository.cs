using FinanceTracker.DAL.Entities.Base;

namespace FinanceTracker.DAL.Repositories.Interfaces;

public interface IRepository<T> where T : BaseEntity, new()
{
    IEnumerable<T> GetAll(bool withRelations = true);
    IAsyncEnumerable<T> GetAllAsync(bool withRelations = true);

    IEnumerable<T> GetAll(Func<T, bool> predicate, bool withRelations = true);
    IAsyncEnumerable<T> GetAllAsync(Func<T, bool> predicate, bool withRelations = true);

    T? GetById(int id, bool withRelations = true);
    Task<T?> GetByIdAsync(int id, bool withRelations = true);

    T? Get(Func<T, bool> predicate, bool withRelations = true);
    Task<T?> GetAsync(Func<T, bool> predicate, bool withRelations = true);


    int Add(T entity);
    Task<int> AddAsync(T entity);

    int AddRange(List<T> entities);
    Task<int> AddRangeAsync(List<T> entities);

    int Update(T entity);
    Task<int> UpdateAsync(T entity);

    int Delete(T entity);
    Task<int> DeleteAsync(T entity);
}