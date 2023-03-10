using System.Collections.Concurrent;
using System.Linq.Expressions;
using FinanceTracker.DatabaseMapper.Descriptors;
using FinanceTracker.DatabaseMapper.Exceptions;
using FinanceTracker.DatabaseMapper.Interfaces;
using Microsoft.Data.SqlClient;

namespace FinanceTracker.DatabaseMapper.Implementations;

public class EntityTableMapper : IEntityTableMapper
{
    private readonly IDictionary<Type, EntityTableDescriptor> _descriptors;
    private readonly IDictionary<Type, Func<SqlDataReader, object>?> _entityBuilders;

    public EntityTableMapper(IEnumerable<EntityTableDescriptor> descriptors)
    {
        _descriptors = new ConcurrentDictionary<Type, EntityTableDescriptor>(
            descriptors.ToDictionary(d => d.Type, d => d)
        );
        _entityBuilders = new ConcurrentDictionary<Type, Func<SqlDataReader, object>?>(
            _descriptors.Select(pair => new KeyValuePair<Type, Func<SqlDataReader, object>?>(pair.Key, null))
        );
    }

    public IEnumerable<string> GetAllColumns<T>()
        where T : class, new()
    {
        return GetDescriptor<T>().MemberColumnDescriptors.Select(d => $"[{d.ColumnName}]");
    }

    public IEnumerable<string> GetColumnsForInsert<T>()
        where T : class, new()
    {
        return GetColumnsWithoutId<T>().Select(c => $"[{c}]");
    }

    public IEnumerable<string> GetParametersForInsert<T>()
        where T : class, new()
    {
        return GetColumnsWithoutId<T>().Select(c => $"@{c}");
    }

    public IEnumerable<string> GetColumnParameterPairsForUpdate<T>()
        where T : class, new()
    {
        return GetColumnsWithoutId<T>().Select(c => $"[{c}] = @{c}");
    }

    public T MapToEntity<T>(SqlDataReader reader)
        where T : class,  new()
    {
        return (T)GetEntityBuilder<T>().Invoke(reader);
    }

    public T MapToEntityWithRelations<T>(SqlDataReader reader)
        where T : class, new()
    {
        throw new NotImplementedException();
    }

    public void SetParameters<T>(SqlCommand command)
        where T : class, new()
    {
        throw new NotImplementedException();
    }

    private EntityTableDescriptor GetDescriptor<T>()
    {
        var type = typeof(T);
        if (!_descriptors.TryGetValue(type, out var descriptor))
        {
            throw new MapNotSupportedException($"Map of type {type} is not supported");
        }

        return descriptor;
    }

    private Func<SqlDataReader, object> GetEntityBuilder<T>()
    {
        var type = typeof(T);
        if (!_entityBuilders.TryGetValue(type, out var entityBuilder))
        {
            throw new MapNotSupportedException($"Map of type {type} is not supported");
        }

        // entityBuilder ??= BuildLambda<T>(reader);
        throw new NotImplementedException();
    }

    private IEnumerable<string> GetColumnsWithoutId<T>()
    {
        return GetDescriptor<T>().MemberColumnDescriptors
            .Where(d => d.MemberName != "Id")
            .Select(d => d.ColumnName);
    }

    public Func<SqlDataReader, T> BuildLambda<T>(SqlDataReader reader)
    {
        var type = typeof(T);
        var descriptor = GetDescriptor<T>();
        var construction = Expression.New(type);
        var variable = Expression.Variable(type, "entity");
        var block = Expression.Block(
            variables: new[] { variable },
            expressions: new[] { Expression.Assign(variable, construction) }
                .Union(descriptor.MemberColumnDescriptors
                    .Select(d => Expression.Assign(
                        Expression.PropertyOrField(variable, d.MemberName),
                        Expression.Constant(reader[d.ColumnName]))))
                .Union(new Expression[] { variable })
        );
        var lambda = Expression.Lambda<Func<SqlDataReader, T>>(block);
        return lambda.Compile();
    }
}