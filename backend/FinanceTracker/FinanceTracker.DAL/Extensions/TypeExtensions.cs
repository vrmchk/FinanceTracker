using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace FinanceTracker.DAL.Extensions;

public static class TypeExtensions
{
    private static readonly IDictionary<Type, PropertyInfo[]> TypeProperties =
        new ConcurrentDictionary<Type, PropertyInfo[]>();

    private static readonly IDictionary<Type, string> SqlColumns = new ConcurrentDictionary<Type, string>();
    public static IDictionary<string, PropertyInfo> GetNamePropertyDictionary(this Type source)
    {
        var properties = GetOrAddProperties(source);
        return new ConcurrentDictionary<string, PropertyInfo>(
            properties.ToDictionary(p => p.GetSqlColumnName(), p => p));
    }

    public static string GetSqlTableName(this Type source)
    {
        return source.GetCustomAttribute<TableAttribute>()?.Name ?? source.Name;
    }

    public static string GetSqlColumnsList(this Type source)
    {
        return PropertiesWithoutId(source)
            .Select(p => $"[{p.Key}]")
            .Aggregate((current, next) => $"{current}, {next}");
    }

    public static string GetSqlParametersList(this Type source)
    {
        return PropertiesWithoutId(source)
            .Select(p => $"@{p.Key}")
            .Aggregate((current, next) => $"{current}, {next}");
    }

    public static string GetSqlUpdateList(this Type source)
    {
        return PropertiesWithoutId(source)
            .Select(p => $"[{p.Key}] = @{p.Key}")
            .Aggregate((current, next) => $"{current}, {next}");
    }

    public static string ListSqlParametersForMultipleEntities(this Type source, int entitiesCount)
    {
        return Enumerable.Range(0, entitiesCount)
            .Select(i => PropertiesWithoutId(source)
                .Select(p => $"@{p.Key}{i}")
                .Aggregate((current, next) => $"{current}, {next}")
            )
            .Aggregate((current, next) => $"{current}\n{next}");
    }

    private static PropertyInfo[] GetOrAddProperties(Type type)
    {
        if (TypeProperties.TryGetValue(type, out var properties))
            return properties;

        properties = type.GetProperties();
        TypeProperties.Add(type, properties);

        return properties;
    }

    private static IEnumerable<KeyValuePair<string, PropertyInfo>> PropertiesWithoutId(Type type)
    {
        return GetOrAddProperties(type)
            .Where(p => p.GetSqlColumnName() != "Id")
            .Select(p => new KeyValuePair<string, PropertyInfo>(p.GetSqlColumnName(), p));
    }
}