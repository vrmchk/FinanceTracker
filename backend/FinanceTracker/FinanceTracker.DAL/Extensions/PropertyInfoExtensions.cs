using System.Reflection;

namespace FinanceTracker.DAL.Extensions;

public static class PropertyInfoExtensions
{
    public static string ListColumns(this IEnumerable<PropertyInfo> source)
    {
        return string.Join(",", WithoutId(source).Select(p => p.Name));
    }

    public static string ListParameters(this IEnumerable<PropertyInfo> source)
    {
        return string.Join(",", WithoutId(source).Select(p => $"@{p.Name}"));
    }

    public static string GetUpdateList(this IEnumerable<PropertyInfo> source)
    {
        return string.Join(",", WithoutId(source).Select(p => $"{p.Name} = @{p.Name}"));
    }
    
    public static string ListParametersForMultipleEntities(this IEnumerable<PropertyInfo> source, int entitiesCount)
    {
        return Enumerable.Range(0, entitiesCount)
            .Select(i => WithoutId(source)
                .Select(p => $"@{p.Name}{i}")
                .Aggregate((current, next) => $"{current},{next}")
            )
            .Aggregate((current, next) => $"{current}\n{next}");
    }

    private static IEnumerable<PropertyInfo> WithoutId(IEnumerable<PropertyInfo> properties)
    {
        return properties.Where(p => p.Name != "Id");
    }
}