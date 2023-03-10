namespace FinanceTracker.DatabaseMapper.Interfaces;

public interface IEntityTableMapperBuilder
{
    IGenericEntityTableDescriptorBuilder<T> CreateMap<T>(string? tableName = null) where T : class, new();
    IEntityTableMapper Build();
}