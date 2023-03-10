using FinanceTracker.DAL.Entities;
using FinanceTracker.DAL.Extensions;
using FinanceTracker.DAL.Repositories.Base;
using FinanceTracker.DAL.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace FinanceTracker.DAL.Repositories;

public class UserRepository : RepositoryBase<User>, IUserRepository
{
    private const string SqlQueryWithJoin = """
            SELECT [U].[Id] AS UserId, [U].[Email], [U].[UserName], [U].[PasswordHash], [U].[EmailConfirmed], 
                   [A].[Id] AS AccountId, [A].[Name] AS AccountName
            FROM [User] [U]
            JOIN [Account] [A] ON [U].[Id] = [A].[Id]
        """;

    public UserRepository(string connectionString) : base(connectionString) { }

    public override IAsyncEnumerable<User> GetAllAsync(bool withRelations = true)
    {
        return GetAllAsync(_ => true, withRelations);
    }

    public override async IAsyncEnumerable<User> GetAllAsync(Func<User, bool> predicate, bool withRelations = true)
    {
        if (withRelations)
        {
            await foreach (var entity in base.GetAllAsync(predicate, withRelations))
            {
                yield return entity;
            }
        }

        await using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        var command = new SqlCommand(SqlQueryWithJoin, connection);
        await using var reader = await command.ExecuteReaderAsync();
        await foreach (var user in MapToUsersWithRelationshipsAsync(reader))
        {
            if (predicate(user))
            {
                yield return user;
            }
        }
    }

    public override async Task<User?> GetByIdAsync(int id, bool withRelations = true)
    {
        if (!withRelations)
        {
            return await base.GetByIdAsync(id, withRelations);
        }

        var sqlQuery = $"{SqlQueryWithJoin} WHERE [U].[Id] = @UserId";
        return await ExecuteAsync(sqlQuery,
            command => command.Parameters.AddWithValue("@UserId", id),
            resultFactory: MapToUserWithRelationshipsAsync
        );
    }

    public override async Task<User?> GetAsync(Func<User, bool> predicate, bool withRelations = true)
    {
        if (!withRelations)
        {
            return await base.GetAsync(predicate, withRelations);
        }

        return await GetAllAsync(predicate, withRelations).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByEmailAsync(string email, bool withRelations = true)
    {
        string sqlQuery;
        if (!withRelations)
        {
            sqlQuery = "SELECT * FROM [User] WHERE [Email] = @Email";
            return await ExecuteAsync(sqlQuery,
                command => command.Parameters.AddWithValue("@Email", email),
                async reader => await reader.ReadAsync() ? reader.MapToObject<User>() : null
            );
        }

        sqlQuery = $"{SqlQueryWithJoin} Where [U].[Email] = @Email";
        return await ExecuteAsync(sqlQuery,
            command => command.Parameters.AddWithValue("@Email", email),
            resultFactory: MapToUserWithRelationshipsAsync
        );
    }

    public async Task<User?> GetByUserNameAsync(string userName, bool withRelations = true)
    {
        string sqlQuery;
        if (!withRelations)
        {
            sqlQuery = "SELECT * FROM [User] WHERE [UserName] = @UserName";
            return await ExecuteAsync(sqlQuery,
                command => command.Parameters.AddWithValue("@UserName", userName),
                async reader => await reader.ReadAsync() ? reader.MapToObject<User>() : null
            );
        }

        sqlQuery = $"{SqlQueryWithJoin} Where [U].[UserName] = @UserName";
        return await ExecuteAsync(sqlQuery,
            command => command.Parameters.AddWithValue("@UserName", userName),
            resultFactory: MapToUserWithRelationshipsAsync
        );
    }


    private async Task<User?> MapToUserWithRelationshipsAsync(SqlDataReader reader)
    {
        var records = await GetDatabaseRecords(reader).ToListAsync();
        var user = records.FirstOrDefault();
        if (user is null)
        {
            return user;
        }

        user.Accounts = records.SelectMany(r => r.Accounts).ToList();
        return user;
    }

    private async IAsyncEnumerable<User> MapToUsersWithRelationshipsAsync(SqlDataReader reader)
    {
        var users = GetDatabaseRecords(reader)
            .GroupBy(u => u.Id)
            .SelectAwait(async group =>
            {
                var user = await group.FirstAsync();
                user.Accounts = group.ToEnumerable().SelectMany(u => u.Accounts).ToList();
                return user;
            });

        await foreach (var user in users)
        {
            yield return user;
        }
    }

    private async IAsyncEnumerable<User> GetDatabaseRecords(SqlDataReader reader)
    {
        while (await reader.ReadAsync())
        {
            var user = new User
            {
                Id = reader.GetInt32(reader.GetOrdinal("UserId")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                UserName = reader.GetString(reader.GetOrdinal("UserName")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                EmailConfirmed = reader.GetBoolean(reader.GetOrdinal("EmailConfirmed")),
                Accounts = new List<Account>()
            };

            var account = new Account
            {
                Id = reader.GetInt32(reader.GetOrdinal("AccountId")),
                Name = reader.GetString(reader.GetOrdinal("AccountName")),
                UserId = user.Id
            };

            user.Accounts.Add(account);

            yield return user;
        }
    }
}