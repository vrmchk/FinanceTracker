using FinanceTracker.DAL.Entities.Base;
using FinanceTracker.DAL.Extensions;
using FinanceTracker.DAL.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace FinanceTracker.DAL.Repositories.Base;

public abstract class RepositoryBase<T> : IRepository<T> where T : BaseEntity, new()
{
    protected readonly string ConnectionString;
    protected readonly string TableName;
    protected readonly Type Type;

    protected RepositoryBase(string connectionString)
    {
        ConnectionString = connectionString;
        Type = typeof(T);
        TableName = Type.GetSqlTableName();
    }

    public virtual IAsyncEnumerable<T> GetAllAsync(bool withRelations = true)
    {
        return GetAllAsync(_ => true, withRelations);
    }

    public virtual async IAsyncEnumerable<T> GetAllAsync(Func<T, bool> predicate, bool withRelations = true)
    {
        await using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        var sqlQuery = $"SELECT * FROM [{TableName}]";
        var command = new SqlCommand(sqlQuery, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var entity = reader.MapToObject<T>();
            if (predicate(entity))
                yield return entity;
        }
    }

    public virtual async Task<T?> GetByIdAsync(int id, bool withRelations = true)
    {
        var sqlQuery = $"SELECT * FROM [{TableName}] WHERE [Id] = @Id";
        return await ExecuteAsync(sqlQuery,
            command => command.Parameters.AddWithValue("@Id", id),
            async reader => await reader.ReadAsync() ? reader.MapToObject<T>() : null);
    }

    public virtual async Task<T?> GetAsync(Func<T, bool> predicate, bool withRelations = true)
    {
        return await GetAllAsync(predicate).FirstOrDefaultAsync();
    }

    public virtual async Task<int> AddAsync(T entity)
    {
        var sqlQuery = $"INSERT INTO [{TableName}] ({Type.GetSqlColumnsList()}) VALUES ({Type.GetSqlParametersList()})";
        return await ExecuteNonQueryAsync(sqlQuery,
            command => command.SetParameters(entity, setId: false)
        );
    }

    public virtual async Task<int> AddRangeAsync(List<T> entities)
    {
        var sqlQuery = $"""
            INSERT INTO [{TableName}] ({Type.GetSqlColumnsList()}) 
            VALUES ({Type.ListSqlParametersForMultipleEntities(entities.Count)})
        """;

        return await ExecuteNonQueryAsync(sqlQuery,
            command => command.SetParametersForMultipleEntities(entities, setId: false)
        );
    }

    public virtual async Task<int> UpdateAsync(T entity)
    {
        var sqlQuery = $"UPDATE [{TableName}] SET {Type.GetSqlUpdateList()} WHERE [Id] = @Id";
        return await ExecuteNonQueryAsync(sqlQuery,
            command => command.SetParameters(entity, setId: true)
        );
    }

    public virtual async Task<int> DeleteAsync(T entity)
    {
        var sqlQuery = $"DELETE FROM [{TableName}] WHERE [Id] = @Id";
        return await ExecuteNonQueryAsync(sqlQuery,
            command => command.Parameters.AddWithValue("@Id", entity.Id)
        );
    }

    protected async Task<TResult?> ExecuteAsync<TResult>(string sqlQuery, Action<SqlCommand>? parametersFactory,
        Func<SqlDataReader, Task<TResult>> resultFactory)
    {
        await using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        var command = new SqlCommand(sqlQuery, connection);
        parametersFactory?.Invoke(command);
        await using var reader = await command.ExecuteReaderAsync();
        return await resultFactory(reader);
    }

    protected async Task<int> ExecuteNonQueryAsync(string sqlQuery, Action<SqlCommand> parametersFactory)
    {
        await using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        var command = new SqlCommand(sqlQuery, connection);
        parametersFactory(command);
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected;
    }
}