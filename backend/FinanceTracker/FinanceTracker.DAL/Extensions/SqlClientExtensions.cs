using FinanceTracker.DAL.Entities.Base;
using Microsoft.Data.SqlClient;

namespace FinanceTracker.DAL.Extensions;

public static class SqlClientExtensions
{
    public static T MapToObject<T>(this SqlDataReader reader)
        where T : BaseEntity, new()
    {
        var type = typeof(T);
        var entity = (T)Activator.CreateInstance(type)!;
        foreach (var (name, property) in type.GetNamePropertyDictionary())
        {
            if (!reader.IsDBNull(reader.GetOrdinal(name)))
            {
                property.SetValue(entity, reader[name]);
            }
        }

        return entity;
    }

    public static void SetParameters<T>(this SqlCommand command, T entity, bool setId)
        where T : BaseEntity, new()
    {
        foreach (var (name, property) in typeof(T).GetNamePropertyDictionary()
                     .Where(pair => setId || pair.Key != "Id"))
        {
            
            command.Parameters.AddWithValue($"@{name}", property.GetValue(entity) ?? DBNull.Value);
        }
    }

    public static void SetParametersForMultipleEntities<T>(this SqlCommand command, List<T> entities, bool setId)
        where T : BaseEntity, new()
    {
        int index = 0;
        foreach (var entity in entities)
        {
            foreach (var (name, property) in typeof(T).GetNamePropertyDictionary()
                         .Where(pair => setId || pair.Key != "Id"))
            {
                command.Parameters.AddWithValue($"@{name}{index++}", property.GetValue(entity) ?? DBNull.Value);
            }
        }
    }
}