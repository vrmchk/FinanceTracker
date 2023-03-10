using System.Linq.Expressions;
using FinanceTracker.DAL.Entities;

namespace FinanceTracker.Tests;

public class ExpressionsTests
{
    [Fact]
    public void SetMemberValue()
    {
        var type = typeof(User);
        var construction = Expression.New(type);
        var variable = Expression.Variable(type, "user");
        var block = Expression.Block(
            variables: new[] { variable },
            Expression.Assign(variable, construction),
            Expression.Assign(Expression.PropertyOrField(variable, "Id"), Expression.Constant(1)),
            variable
        );
        var lambda = Expression.Lambda<Func<User>>(block);
        var result = lambda.Compile()();
        Assert.True(result.Id == 1);
    }

    [Fact]
    public void GetMemberValue()
    {
        var user = new User { Id = 1 };
        var property = Expression.PropertyOrField(Expression.Constant(user), "Id");
        var lambda = Expression.Lambda<Func<int>>(property);
        var result = lambda.Compile()();
        Assert.True(result == 1);
    }

    [Fact]
    public void MemberExists()
    {
        bool result;
        try
        {
            var member = Expression.PropertyOrField(Expression.New(typeof(User)), "Id");
            result = true;
        }
        catch
        {
            result = false;
        }
        Assert.True(result);
    }
    
    Func<T> Example<T>()
    {
        var type = typeof(T);
        var construction = Expression.New(type);
        var variable = Expression.Variable(type);
        var block = Expression.Block(
            variables: new[] { variable },
            Expression.Assign(variable, construction),
            Expression.Assign(Expression.PropertyOrField(variable, "first"), Expression.Constant("first")),
            Expression.Assign(Expression.PropertyOrField(variable, "second"), Expression.Constant("second")),
            variable
        );

        var lambda = Expression.Lambda<Func<T>>(block);
        return lambda.Compile();
    }
}