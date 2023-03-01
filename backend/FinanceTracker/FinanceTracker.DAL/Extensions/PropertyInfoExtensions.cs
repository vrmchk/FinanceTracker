using System.Reflection;

namespace FinanceTracker.DAL.Extensions;

public static class PropertyInfoExtensions
{
    public static string ListColumns(this IDictionary<string, PropertyInfo> source)
    {
        return string.Join(", ", WithoutId(source).Select(p => $"[{p.Key}]"));
    }

    public static string ListParameters(this IDictionary<string, PropertyInfo> source)
    {
        return string.Join(", ", WithoutId(source).Select(p => $"@{p.Key}"));
    }

    public static string GetUpdateList(this IDictionary<string, PropertyInfo> source)
    {
        return string.Join(", ", WithoutId(source).Select(p => $"[{p.Key}] = @{p.Key}"));
    }

    public static string ListParametersForMultipleEntities(this IDictionary<string, PropertyInfo> source,
        int entitiesCount)
    {
        return Enumerable.Range(0, entitiesCount)
            .Select(i => WithoutId(source)
                .Select(p => $"@{p.Key}{i}")
                .Aggregate((current, next) => $"{current}, {next}")
            )
            .Aggregate((current, next) => $"{current}\n{next}");
    }

    private static IEnumerable<KeyValuePair<string, PropertyInfo>> WithoutId(
        IDictionary<string, PropertyInfo> properties)
    {
        return properties.Where(p => p.Key != "Id");
    }
}