using FinanceTracker.DatabaseMapper.Interfaces;

namespace FinanceTracker.DatabaseMapper.Implementations;

public class EntityTableMapperBuilder : IEntityTableMapperBuilder
{
    private readonly List<IEntityTypeDescriptorBuilder> _descriptorBuilders = new();

    public IGenericEntityTableDescriptorBuilder<T> CreateMap<T>(string? tableName = null)
        where T : class, new()
    {
        var entityMapperBuilder = new EntityTableDescriptorBuilder<T>(tableName);
        _descriptorBuilders.Add(entityMapperBuilder);
        return entityMapperBuilder;
    }

    public IEntityTableMapper Build()
    {
        var descriptors = _descriptorBuilders.Select(b => b.Build());
        return new EntityTableMapper(descriptors);
    }
}