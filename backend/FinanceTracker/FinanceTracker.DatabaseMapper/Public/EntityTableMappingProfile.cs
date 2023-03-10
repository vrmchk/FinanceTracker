using FinanceTracker.DatabaseMapper.Implementations;
using FinanceTracker.DatabaseMapper.Interfaces;

namespace FinanceTracker.DatabaseMapper.Public;

public abstract class EntityTableMappingProfile
{
    private readonly EntityTableMapperBuilder _entityTableMapperBuilder = new();

    protected abstract void CreateMaps();
    
    protected IGenericEntityTableDescriptorBuilder<T> CreateMap<T>(string? tableName = null)
        where T : class, new()
    {
        return _entityTableMapperBuilder.CreateMap<T>(tableName);
    }
}