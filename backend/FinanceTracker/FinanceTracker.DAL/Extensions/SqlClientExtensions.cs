using System.Reflection;
using FinanceTracker.DAL.Entities.Base;
using Microsoft.Data.SqlClient;

namespace FinanceTracker.DAL.Extensions;

public static class SqlClientExtensions
{
    public static T MapToObject<T>(this SqlDataReader reader, PropertyInfo[]? properties = null)
        where T : BaseEntity, new()
    {
        properties ??= typeof(T).GetProperties();
        var entity = (T)Activator.CreateInstance(typeof(T))!;
        foreach (var property in properties)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(property.Name)))
            {
                property.SetValue(entity, reader[property.Name]);
            }
        }

        return entity;
    }

    public static void SetIdParameter(this SqlCommand command, int id)
    {
        command.Parameters.AddWithValue("@Id", id);
    }

    public static void SetParameters<T>(this SqlCommand command, T entity, bool setId,
        PropertyInfo[]? properties = null)
        where T : BaseEntity, new()
    {
        properties ??= typeof(T).GetProperties();
        properties = properties.Where(p => setId || p.Name != "Id").ToArray();
        foreach (var property in properties)
        {
            command.Parameters.AddWithValue($"@{property.Name}", property.GetValue(entity) ?? DBNull.Value);
        }
    }

    public static void SetParametersForMultipleEntities<T>(this SqlCommand command, List<T> entities, bool setId,
        PropertyInfo[]? properties = null)
        where T : BaseEntity, new()
    {
        properties ??= typeof(T).GetProperties();
        properties = properties.Where(p => setId || p.Name != "Id").ToArray();
        int index = 0;
        foreach (var entity in entities)
        {
            foreach (var property in properties)
            {
                command.Parameters.AddWithValue($"@{property.Name}{index++}",
                    property.GetValue(entity) ?? DBNull.Value);
            }
        }
    }
}