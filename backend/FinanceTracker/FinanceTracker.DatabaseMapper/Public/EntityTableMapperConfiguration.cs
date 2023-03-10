using FinanceTracker.DatabaseMapper.Interfaces;

namespace FinanceTracker.DatabaseMapper.Public;

public class EntityTableMapperConfiguration
{
    private readonly List<EntityTableMappingProfile> _profiles;

    public EntityTableMapperConfiguration(IEnumerable<EntityTableMappingProfile> profiles)
    {
        _profiles = new List<EntityTableMappingProfile>().ToList();
    }

    public EntityTableMapperConfiguration(params EntityTableMappingProfile[] profiles)
    {
        _profiles = profiles.ToList();
    }

    public IEntityTableMapper CreateMapper()
    {
        throw new NotImplementedException();
    }
}