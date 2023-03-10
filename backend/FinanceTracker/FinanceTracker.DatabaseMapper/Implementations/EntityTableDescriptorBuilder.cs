using System.Linq.Expressions;
using FinanceTracker.DatabaseMapper.Descriptors;
using FinanceTracker.DatabaseMapper.Exceptions;
using FinanceTracker.DatabaseMapper.Interfaces;

namespace FinanceTracker.DatabaseMapper.Implementations;

public class EntityTableDescriptorBuilder<T> : IGenericEntityTableDescriptorBuilder<T> where T : class, new()
{
    private readonly EntityTableDescriptor _descriptor;

    public EntityTableDescriptorBuilder(string? tableName = null)
    {
        var type = typeof(T);
        _descriptor = new EntityTableDescriptor
        {
            Type = type,
            TableName = tableName ?? type.Name
        };
    }

    public IGenericEntityTableDescriptorBuilder<T> AddMapping(string columnName, string sourceMemberName)
    {
        var memberDescriptor = new MemberColumnDescriptor
        {
            MemberName = sourceMemberName,
            ColumnName = columnName
        };
        ThrowIfMemberDescriptorIsNotValid(memberDescriptor);
        _descriptor.MemberColumnDescriptors.Add(memberDescriptor);
        return this;
    }

    public IGenericEntityTableDescriptorBuilder<T> AddMapping<TMember>(string columnName,
        Expression<Func<T, TMember>> sourceMember)
    {
        return AddMapping(columnName, GetMemberName(sourceMember));
    }

    public IGenericEntityTableDescriptorBuilder<T> AddMapping(string sourceMemberName)
    {
        return AddMapping(sourceMemberName, sourceMemberName);
    }

    public IGenericEntityTableDescriptorBuilder<T> AddMapping<TMember>(Expression<Func<T, TMember>> sourceMember)
    {
        var memberName = GetMemberName(sourceMember);
        return AddMapping(memberName, memberName);
    }

    public EntityTableDescriptor Build() => _descriptor;

    private string GetMemberName<TMember>(Expression<Func<T, TMember>> sourceMember)
    {
        return ((MemberExpression)sourceMember.Body).Member.Name;
    }

    private void ThrowIfMemberDescriptorIsNotValid(MemberColumnDescriptor memberDescriptor)
    {
        if (!MemberExists(memberDescriptor.MemberName))
            throw new MemberDoesntExistException($"Member with name {memberDescriptor.MemberName} does not exist",
                nameof(memberDescriptor.MemberName));

        var existingDescriptor =
            _descriptor.MemberColumnDescriptors.FirstOrDefault(d => d.MemberName == memberDescriptor.MemberName);
        if (existingDescriptor != null)
            throw new SameMapAlreadyAddedException($"Member with name {memberDescriptor.MemberName} already added",
                nameof(memberDescriptor.MemberName));

        existingDescriptor =
            _descriptor.MemberColumnDescriptors.FirstOrDefault(d => d.ColumnName == memberDescriptor.ColumnName);
        if (existingDescriptor != null)
            throw new SameMapAlreadyAddedException($"Column with name {memberDescriptor.ColumnName} already added",
                nameof(memberDescriptor.ColumnName));
    }

    private bool MemberExists(string memberName)
    {
        try
        {
            var member = Expression.PropertyOrField(Expression.New(typeof(T)), memberName);
            return true;
        }
        catch
        {
            return false;
        }
    }
}