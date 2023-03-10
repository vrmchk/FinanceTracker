using FinanceTracker.DAL.Entities;
using FinanceTracker.DatabaseMapper.Implementations;
using FinanceTracker.DatabaseMapper.Interfaces;
using Microsoft.Data.SqlClient;

namespace FinanceTracker.Tests;

public class EntityTableMapperTests
{
    private readonly IEntityTableMapperBuilder _entityTableBuilder = new EntityTableMapperBuilder();
    private readonly IEntityTableMapper _entityTableMapper;

    private readonly string[] _userProperties =
    {
        "Id",
        "Email",
        "UserName",
        "PasswordHash",
        "EmailConfirmed"
    };

    public EntityTableMapperTests()
    {
        _entityTableBuilder.CreateMap<User>()
            .AddMapping(u => u.Id)
            .AddMapping("Email")
            .AddMapping("UserName", u => u.UserName)
            .AddMapping("PasswordHash", "PasswordHash")
            .AddMapping(u => u.EmailConfirmed);

        _entityTableMapper = _entityTableBuilder.Build();
    }

    [Fact]
    public void EntityTableMapper_MapsAllColumns()
    {
        var columns = _entityTableMapper.GetAllColumns<User>().ToArray();
        Assert.True(columns.Intersect(_userProperties.Select(p => $"[{p}]")).Count() == columns.Length);
    }

    [Fact]
    public void EntityTableMapper_MapsColumnsForInsert()
    {
        var columns = _entityTableMapper.GetColumnsForInsert<User>().ToArray();
        Assert.True(columns.Intersect(_userProperties.Except(new[] { "Id" })
                .Select(p => $"[{p}]")).Count() == columns.Length
        );
    }

    [Fact]
    public void EntityTableMapper_MapsParametersForInsert()
    {
        var columns = _entityTableMapper.GetParametersForInsert<User>().ToArray();
        Assert.True(columns.Intersect(_userProperties.Except(new[] { "Id" })
                .Select(p => $"@{p}")).Count() == columns.Length
        );
    }

    [Fact]
    public void EntityTableMapper_MapsColumnParameterPairsForUpdate()
    {
        var columns = _entityTableMapper.GetColumnParameterPairsForUpdate<User>().ToArray();
        Assert.True(columns.Intersect(_userProperties.Except(new[] { "Id" })
                .Select(p => $"[{p}] = @{p}")).Count() == columns.Length
        );
    }

    [Fact]
    public void EntityTableMapper_BuildsLambda()
    {
        using var connection =
            new SqlConnection(
                "Server=localhost;Database=FinanceTracker;Trusted_Connection=True;TrustServerCertificate=True;");
        connection.Open();
        var sqlQuery =
            $"SELECT {_entityTableMapper.GetAllColumns<User>().Aggregate((curr, next) => curr + ", " + next)} FROM [User] WHERE [Id] = 1";
        using var command = new SqlCommand(sqlQuery, connection);
        var reader = command.ExecuteReader();
        Assert.True(reader.Read());
        var func = ((EntityTableMapper)_entityTableMapper).BuildLambda<User>(reader);
        var user = func(reader);
        Assert.True(user.Id == 1);
    }
}