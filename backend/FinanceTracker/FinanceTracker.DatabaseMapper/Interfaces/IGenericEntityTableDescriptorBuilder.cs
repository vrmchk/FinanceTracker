using System.Linq.Expressions;

namespace FinanceTracker.DatabaseMapper.Interfaces;

public interface IGenericEntityTableDescriptorBuilder<T> : IEntityTypeDescriptorBuilder
    where T : class, new()
{
    IGenericEntityTableDescriptorBuilder<T> AddMapping(string columnName, string sourceMemberName);

    IGenericEntityTableDescriptorBuilder<T> AddMapping<TMember>(string columnName,
        Expression<Func<T, TMember>> sourceMember);

    IGenericEntityTableDescriptorBuilder<T> AddMapping(string sourceMemberName);
    IGenericEntityTableDescriptorBuilder<T> AddMapping<TMember>(Expression<Func<T, TMember>> sourceMember);
}