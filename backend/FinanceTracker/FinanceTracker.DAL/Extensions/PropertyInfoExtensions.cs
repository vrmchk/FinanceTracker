using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace FinanceTracker.DAL.Extensions;

public static class PropertyInfoExtensions
{
    private static readonly IDictionary<PropertyInfo, string> PropertiesColumnNames =
        new ConcurrentDictionary<PropertyInfo, string>();

    public static string GetSqlColumnName(this PropertyInfo source)
    {
        if (PropertiesColumnNames.TryGetValue(source, out var columnName))
            return columnName;

        columnName = source.GetCustomAttribute<ColumnAttribute>()?.Name ?? source.Name;
        PropertiesColumnNames.Add(source, columnName);
        return columnName;
    }
}