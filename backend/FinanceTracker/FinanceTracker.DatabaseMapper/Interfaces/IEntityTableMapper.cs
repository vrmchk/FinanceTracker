using Microsoft.Data.SqlClient;

namespace FinanceTracker.DatabaseMapper.Interfaces;

public interface IEntityTableMapper
{
    IEnumerable<string> GetAllColumns<T>() where T : class, new();
    IEnumerable<string> GetColumnsForInsert<T>() where T : class, new();
    IEnumerable<string> GetParametersForInsert<T>() where T : class, new();
    IEnumerable<string> GetColumnParameterPairsForUpdate<T>() where T : class, new();
    T MapToEntity<T>(SqlDataReader reader) where T : class, new();
    T MapToEntityWithRelations<T>(SqlDataReader reader) where T : class, new();
    void SetParameters<T>(SqlCommand command) where T : class, new();
}