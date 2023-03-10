using FinanceTracker.DatabaseMapper.Descriptors;

namespace FinanceTracker.DatabaseMapper.Interfaces;

public interface IEntityTypeDescriptorBuilder
{
    EntityTableDescriptor Build();
}