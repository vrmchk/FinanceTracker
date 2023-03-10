using System.Reflection;
using FinanceTracker.DAL.Entities;
using FinanceTracker.DAL.Extensions;

namespace Tests;

public class TypeExtensionsTests
{
    [Fact]
    public void GetNamePropertyName_ReturnsDifferentResults_WhenTypesAreDifferent()
    {
        var userProperties = typeof(User).GetNamePropertyDictionary();
        var accountProperties = typeof(Account).GetNamePropertyDictionary();
        Assert.False(userProperties.Keys.SequenceEqual(accountProperties.Keys));
    }

    [Fact]
    public void GetPropertyName_ReturnsDifferentResults_WhenTypesAreDifferent()
    {
        var userColumns = typeof(User).GetSqlColumnsList();
        var accountColumns = typeof(Account).GetSqlColumnsList();
        Assert.False(userColumns == accountColumns);
    }

    [Fact]
    public void TestType()
    {
        var systemType = typeof(string);
        var userDefinedType = typeof(User);
        Assert.True(typeof(int[]).IsPrimitive);
        // Assert.True(IsUserDefined(userDefinedType));
        // Assert.False(IsUserDefined(systemType));
    }

    private bool IsUserDefined(Type type)
    {
        return type.Assembly.FullName == Assembly.GetExecutingAssembly().FullName;
    }
}