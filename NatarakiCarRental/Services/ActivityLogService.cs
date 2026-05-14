using Dapper;
using NatarakiCarRental.Data;

namespace NatarakiCarRental.Services;

public sealed class ActivityLogService
{
    private readonly DbConnectionFactory _connectionFactory;

    public ActivityLogService()
        : this(new DbConnectionFactory())
    {
    }

    public ActivityLogService(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task LogAsync(string actionType, string entityName, int? entityId, string description, int? userId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actionType);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        const string sql = """
            INSERT INTO dbo.ActivityLogs
            (
                UserId,
                ActionType,
                EntityName,
                EntityId,
                Description
            )
            VALUES
            (
                @UserId,
                @ActionType,
                @EntityName,
                @EntityId,
                @Description
            );
            """;

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            sql,
            new
            {
                UserId = userId,
                ActionType = actionType.Trim(),
                EntityName = string.IsNullOrWhiteSpace(entityName) ? null : entityName.Trim(),
                EntityId = entityId,
                Description = description.Trim()
            });
    }
}
