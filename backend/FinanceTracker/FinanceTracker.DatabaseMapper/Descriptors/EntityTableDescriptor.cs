namespace FinanceTracker.DatabaseMapper.Descriptors;

public class EntityTableDescriptor
{
    public required Type Type { get; init; }
    public required string TableName { get; init; }
    public List<MemberColumnDescriptor> MemberColumnDescriptors { get; } = new();
}