using Dapper;
using NatarakiCarRental.Data;
using NatarakiCarRental.Models;

namespace NatarakiCarRental.Repositories;

public sealed class UserRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public UserRepository()
        : this(new DbConnectionFactory())
    {
    }

    public UserRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public User? GetActiveUserByUsername(string username)
    {
        const string sql = """
            SELECT
                UserId,
                RoleId,
                Username,
                PasswordHash,
                FirstName,
                LastName,
                Email,
                PhoneNumber,
                IsActive,
                IsArchived,
                CreatedAt,
                UpdatedAt,
                ArchivedAt
            FROM dbo.Users
            WHERE Username = @Username
              AND IsActive = 1
              AND IsArchived = 0;
            """;

        using var connection = _connectionFactory.CreateConnection();
        return connection.QuerySingleOrDefault<User>(sql, new { Username = username });
    }
}
