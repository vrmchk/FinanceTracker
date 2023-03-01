using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using FinanceTracker.DAL.Entities.Base;
using FinanceTracker.DAL.Extensions;
using FinanceTracker.DAL.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace FinanceTracker.DAL.Repositories.Base;

public abstract class RepositoryBase<T> : IRepository<T> where T : BaseEntity, new()
{
    protected readonly string ConnectionString;
    protected readonly string TableName;
    protected readonly IDictionary<string, PropertyInfo> PropertiesNames;

    protected RepositoryBase(string connectionString)
    {
        ConnectionString = connectionString;
        var type = typeof(T);
        TableName = type.GetCustomAttribute<TableAttribute>()?.Name ?? type.Name;
        PropertiesNames = new ConcurrentDictionary<string, PropertyInfo>(type.GetProperties()
            .ToDictionary(
                p => p.GetCustomAttribute<ColumnAttribute>()?.Name ?? p.Name,
                p => p));
    }

    public virtual IEnumerable<T> GetAll(bool withRelations = true)
    {
        return GetAll(_ => true, withRelations);
    }

    public virtual async IAsyncEnumerable<T> GetAllAsync(bool withRelations = true)
    {
        await foreach (var entity in GetAllAsync(_ => true, withRelations))
        {
            yield return entity;
        }
    }

    public IEnumerable<T> GetAll(Func<T, bool> predicate, bool withRelations = true)
    {
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        var sql = $"SELECT * FROM [{TableName}]";
        var command = new SqlCommand(sql, connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var entity = reader.MapToObject<T>(PropertiesNames);
            if (predicate(entity))
                yield return entity;
        }
    }

    public async IAsyncEnumerable<T> GetAllAsync(Func<T, bool> predicate, bool withRelations = true)
    {
        await using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        var sql = $"SELECT * FROM [{TableName}]";
        var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var entity = reader.MapToObject<T>(PropertiesNames);
            if (predicate(entity))
                yield return entity;
        }
    }

    public virtual T? GetById(int id, bool withRelations = true)
    {
        var sql = $"SELECT * FROM [{TableName}] WHERE [Id] = @Id";
        return Execute(sql,
            command => command.SetIdParameter(id),
            reader => reader.Read() ? reader.MapToObject<T>(PropertiesNames) : null);
    }

    public virtual async Task<T?> GetByIdAsync(int id, bool withRelations = true)
    {
        var sql = $"SELECT * FROM [{TableName}] WHERE [Id] = @Id";
        return await ExecuteAsync(sql,
            command => command.SetIdParameter(id),
            async reader => await reader.ReadAsync() ? reader.MapToObject<T>(PropertiesNames) : null);
    }

    public virtual T? Get(Func<T, bool> predicate, bool withRelations = true)
    {
        var sql = $"SELECT * FROM [{TableName}]";
        return Execute(sql, null, reader =>
        {
            while (reader.Read())
            {
                var entity = reader.MapToObject<T>(PropertiesNames);
                if (predicate(entity))
                    return entity;
            }

            return null;
        });
    }

    public virtual async Task<T?> GetAsync(Func<T, bool> predicate, bool withRelations = true)
    {
        var sql = $"SELECT * FROM [{TableName}]";
        return await ExecuteAsync(sql, null, async reader =>
        {
            while (await reader.ReadAsync())
            {
                var entity = reader.MapToObject<T>(PropertiesNames);
                if (predicate(entity))
                    return entity;
            }

            return null;
        });
    }

    public virtual int Add(T entity)
    {
        var sql = $"""
            INSERT INTO [{TableName}] ({PropertiesNames.ListColumns()}) 
            VALUES ({PropertiesNames.ListParameters()})
        """;
        return ExecuteNonQuery(sql,
            command => command.SetParameters(entity, PropertiesNames, setId: false)
        );
    }

    public virtual async Task<int> AddAsync(T entity)
    {
        var sql = $"""
            INSERT INTO [{TableName}] ({PropertiesNames.ListColumns()}) 
            VALUES ({PropertiesNames.ListParameters()})
        """;
        return await ExecuteNonQueryAsync(sql,
            command => command.SetParameters(entity, PropertiesNames, setId: false)
        );
    }

    public virtual int AddRange(List<T> entities)
    {
        var sql = $"""
            INSERT INTO [{TableName}] ({PropertiesNames.ListColumns()}) 
            VALUES ({PropertiesNames.ListParameters()})
        """;

        return ExecuteNonQuery(sql,
            command => command.SetParametersForMultipleEntities(entities, PropertiesNames, setId: false)
        );
    }

    public virtual async Task<int> AddRangeAsync(List<T> entities)
    {
        var sql = $"""
            INSERT INTO [{TableName}] ({PropertiesNames.ListColumns()}) 
            VALUES ({PropertiesNames.ListParametersForMultipleEntities(entities.Count)})
        """;

        return await ExecuteNonQueryAsync(sql,
            command => command.SetParametersForMultipleEntities(entities, PropertiesNames, setId: false)
        );
    }

    public virtual int Update(T entity)
    {
        var sql = $"UPDATE [{TableName}] SET {PropertiesNames.GetUpdateList()} WHERE [Id] = @Id";
        return ExecuteNonQuery(sql,
            command => command.SetParameters(entity, PropertiesNames, setId: true)
        );
    }

    public virtual async Task<int> UpdateAsync(T entity)
    {
        var sql = $"UPDATE [{TableName}] SET {PropertiesNames.GetUpdateList()} WHERE [Id] = @Id";
        return await ExecuteNonQueryAsync(sql,
            command => command.SetParameters(entity, PropertiesNames, setId: true)
        );
    }

    public virtual int Delete(T entity)
    {
        var sql = $"DELETE FROM [{TableName}] WHERE [Id] = @Id";
        return ExecuteNonQuery(sql,
            command => command.Parameters.AddWithValue("@Id", entity.Id)
        );
    }

    public virtual async Task<int> DeleteAsync(T entity)
    {
        var sql = $"DELETE FROM [{TableName}] WHERE [Id] = @Id";
        return await ExecuteNonQueryAsync(sql,
            command => command.Parameters.AddWithValue("@Id", entity.Id)
        );
    }

    protected TResult Execute<TResult>(string sql, Action<SqlCommand>? parametersFactory,
        Func<SqlDataReader, TResult> resultFactory)
    {
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        var command = new SqlCommand(sql, connection);
        parametersFactory?.Invoke(command);
        using var reader = command.ExecuteReader();
        return resultFactory(reader);
    }

    protected async Task<TResult> ExecuteAsync<TResult>(string sql, Action<SqlCommand>? parametersFactory,
        Func<SqlDataReader, Task<TResult>> resultFactory)
    {
        await using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        var command = new SqlCommand(sql, connection);
        parametersFactory?.Invoke(command);
        await using var reader = await command.ExecuteReaderAsync();
        return await resultFactory(reader);
    }

    protected int ExecuteNonQuery(string sql, Action<SqlCommand> parametersFactory)
    {
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        var command = new SqlCommand(sql, connection);
        parametersFactory(command);
        var rowsAffected = command.ExecuteNonQuery();
        return rowsAffected;
    }

    protected async Task<int> ExecuteNonQueryAsync(string sql, Action<SqlCommand> parametersFactory)
    {
        await using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        var command = new SqlCommand(sql, connection);
        parametersFactory(command);
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected;
    }
}