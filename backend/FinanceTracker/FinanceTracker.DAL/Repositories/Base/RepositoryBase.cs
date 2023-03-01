using System.Reflection;
using FinanceTracker.DAL.Entities.Base;
using FinanceTracker.DAL.Extensions;
using FinanceTracker.DAL.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace FinanceTracker.DAL.Repositories.Base;

public abstract class RepositoryBase<T> : IRepository<T> where T : BaseEntity, new()
{
    protected readonly string ConnectionString;
    protected readonly PropertyInfo[] Properties;
    
    protected RepositoryBase(string connectionString)
    {
        ConnectionString = connectionString;
        Properties = typeof(T).GetProperties();
    }

    protected abstract string TableName { get; }

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
        var sql = $"SELECT * FROM {TableName}";
        var command = new SqlCommand(sql, connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var entity = reader.MapToObject<T>(Properties);
            if (predicate(entity))
                yield return entity;
        }
    }

    public async IAsyncEnumerable<T> GetAllAsync(Func<T, bool> predicate, bool withRelations = true)
    {
        await using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        var sql = $"SELECT * FROM {TableName}";
        var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var entity = reader.MapToObject<T>(Properties);
            if (predicate(entity))
                yield return entity;
        }
    }

    public virtual T? GetById(int id, bool withRelations = true)
    {
        var sql = $"SELECT * FROM {TableName} WHERE Id = @Id";
        return Execute(sql,
            command => command.SetIdParameter(id),
            reader => reader.Read() ? reader.MapToObject<T>(Properties) : null);
    }

    public virtual async Task<T?> GetByIdAsync(int id, bool withRelations = true)
    {
        var sql = $"SELECT * FROM {TableName} WHERE Id = @Id";
        return await ExecuteAsync(sql,
            command => command.SetIdParameter(id),
            async reader => await reader.ReadAsync() ? reader.MapToObject<T>(Properties) : null);
    }

    public virtual T? Get(Func<T, bool> predicate, bool withRelations = true)
    {
        var sql = $"SELECT * FROM {TableName}";
        return Execute(sql, null, reader =>
        {
            while (reader.Read())
            {
                var entity = reader.MapToObject<T>(Properties);
                if (predicate(entity))
                    return entity;
            }

            return null;
        });
    }

    public virtual async Task<T?> GetAsync(Func<T, bool> predicate, bool withRelations = true)
    {
        var sql = $"SELECT * FROM {TableName}";
        return await ExecuteAsync(sql, null, async reader =>
        {
            while (await reader.ReadAsync())
            {
                var entity = reader.MapToObject<T>(Properties);
                if (predicate(entity))
                    return entity;
            }

            return null;
        });
    }

    public virtual int Add(T entity)
    {
        var sql = $"INSERT INTO {TableName} ({Properties.ListColumns()}) VALUES ({Properties.ListParameters()})";
        return ExecuteNonQuery(sql,
            command => command.SetParameters(entity, setId: false, Properties)
        );
    }

    public virtual async Task<int> AddAsync(T entity)
    {
        var sql = $"INSERT INTO {TableName} ({Properties.ListColumns()}) VALUES ({Properties.ListParameters()})";
        return await ExecuteNonQueryAsync(sql,
            command => command.SetParameters(entity, setId: false, Properties)
        );
    }

    public virtual int AddRange(List<T> entities)
    {
        var sql =
            $"INSERT INTO {TableName} ({Properties.ListColumns()}) VALUES ({Properties.ListParametersForMultipleEntities(entities.Count)})";

        return ExecuteNonQuery(sql,
            command =>  command.SetParametersForMultipleEntities(entities, setId: false, Properties)
        );
    }

    public virtual async Task<int> AddRangeAsync(List<T> entities)
    {
        var sql =
            $"INSERT INTO {TableName} ({Properties.ListColumns()}) VALUES ({Properties.ListParametersForMultipleEntities(entities.Count)})";

        return await ExecuteNonQueryAsync(sql,
            command =>  command.SetParametersForMultipleEntities(entities, setId: false, Properties)
        );
    }

    public virtual int Update(T entity)
    {
        var sql = $"UPDATE {TableName} SET {Properties.GetUpdateList()} WHERE Id = @Id";
        return ExecuteNonQuery(sql,
            command => command.SetParameters(entity, setId: true, Properties)
        );
    }

    public virtual async Task<int> UpdateAsync(T entity)
    {
        var sql = $"UPDATE {TableName} SET {Properties.GetUpdateList()} WHERE Id = @Id";
        return await ExecuteNonQueryAsync(sql,
            command => command.SetParameters(entity, setId: true, Properties)
        );
    }

    public virtual int Delete(T entity)
    {
        var sql = $"DELETE FROM {TableName} WHERE Id = @Id";
        return ExecuteNonQuery(sql,
            command => command.Parameters.AddWithValue("@Id", entity.Id)
        );
    }

    public virtual async Task<int> DeleteAsync(T entity)
    {
        var sql = $"DELETE FROM {TableName} WHERE Id = @Id";
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

    /*private string ListColumns()
    {
        return string.Join(",", PropertiesWithoutId.Select(p => p.Name));
    }
    
    private string ListParameters()
    {
        return string.Join(",", PropertiesWithoutId.Select(p => $"@{p.Name}"));
    }
    
    private string GetUpdateList()
    {
        return string.Join(",", PropertiesWithoutId.Select(p => $"{p.Name} = @{p.Name}"));
    }
    
    private string ListParametersForMultipleEntities(int entitiesCount)
    {
        return Enumerable.Range(0, entitiesCount)
            .Select(i => PropertiesWithoutId
                .Select(p => $"@{p.Name}{i}")
                .Aggregate((current, next) => $"{current},{next}")
            )
            .Aggregate((current, next) => $"{current}\n{next}");
    }

    public virtual T? GetById(int id, bool withRelations = true)
    {
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        var sql = $"SELECT * FROM {TableName} WHERE Id = @Id";
        var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        using var reader = command.ExecuteReader();
    
        return reader.Read() ? MapToObject(reader) : null;
    }

    public virtual int Add(T entity)
    {
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        var sql = $"INSERT INTO {TableName} ({GetColumnList()}) VALUES ({GetParameterList()})";
        var command = new SqlCommand(sql, connection);
        SetParameters(command, entity);
        var rowsAffected = command.ExecuteNonQuery();
        return rowsAffected;
    }

    public virtual int Update(T entity)
    {
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        var sql = $"UPDATE {TableName} SET {GetUpdateList()} WHERE Id = @Id";
        var command = new SqlCommand(sql, connection);
        SetParameters(command, entity);
        command.Parameters.AddWithValue("@Id", entity.Id);
        var rowsAffected = command.ExecuteNonQuery();
        return rowsAffected;
    }

    public virtual int Delete(T entity)
    {
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        var sql = $"DELETE FROM {TableName} WHERE Id = @Id";
        var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", entity.Id);
        var rowsAffected = command.ExecuteNonQuery();
        return rowsAffected;
    }

    private T MapToObject(SqlDataReader reader)
    {
        var entity = (T)Activator.CreateInstance(typeof(T))!;
        foreach (var property in Properties)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(property.Name)))
            {
                property.SetValue(entity, reader[property.Name]);
            }
        }
    
        return entity;
    }

    private void SetParameters(SqlCommand command, T entity, bool setId)
    {
        var properties = setId ? Properties : PropertiesWithoutId;
        foreach (var property in properties)
        {
            command.Parameters.AddWithValue($"@{property.Name}", property.GetValue(entity) ?? DBNull.Value);
        }
    }

    private void SetIdParameter(SqlCommand command, int id)
    {
        command.Parameters.AddWithValue("@Id", id);
    }

    private void SetParametersForMultipleEntities(SqlCommand command, List<T> entities)
    {
        int index = 0;
        foreach (var entity in entities)
        {
            foreach (var property in PropertiesWithoutId)
            {
                command.Parameters.AddWithValue($"@{property.Name}{index++}",
                    property.GetValue(entity) ?? DBNull.Value);
            }
        }
    }*/
}