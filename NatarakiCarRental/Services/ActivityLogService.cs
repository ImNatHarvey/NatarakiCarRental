using System.Data;
using Dapper;
using NatarakiCarRental.Data;
using NatarakiCarRental.Models;
using NatarakiCarRental.Repositories;

namespace NatarakiCarRental.Services;

public sealed class ActivityLogService
{
    private readonly DbConnectionFactory _connectionFactory;
    private readonly ActivityLogRepository _activityLogRepository;

    public ActivityLogService()
        : this(new DbConnectionFactory())
    {
    }

    public ActivityLogService(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
        _activityLogRepository = new ActivityLogRepository(connectionFactory);
    }

    public Task<IReadOnlyList<ActivityLog>> SearchLogsAsync(
        string searchText,
        string? actionType = null,
        string? entityName = null,
        int maxRows = 100)
    {
        return _activityLogRepository.SearchLogsAsync(searchText, actionType, entityName, maxRows);
    }

    public Task<ActivityLogMetrics> GetMetricsAsync()
    {
        return _activityLogRepository.GetMetricsAsync();
    }

    public Task<IReadOnlyList<string>> GetActionTypesAsync()
    {
        return _activityLogRepository.GetActionTypesAsync();
    }

    public Task<IReadOnlyList<string>> GetEntityNamesAsync()
    {
        return _activityLogRepository.GetEntityNamesAsync();
    }

    public async Task LogAsync(
        string actionType,
        string entityName,
        int? entityId,
        string description,
        int? userId = null,
        IDbTransaction? transaction = null)
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

        IDbConnection connection = transaction?.Connection ?? _connectionFactory.CreateConnection();

        try
        {
            await connection.ExecuteAsync(
                sql,
                new
                {
                    UserId = userId,
                    ActionType = actionType.Trim(),
                    EntityName = string.IsNullOrWhiteSpace(entityName) ? null : entityName.Trim(),
                    EntityId = entityId,
                    Description = description.Trim()
                },
                transaction);
        }
        finally
        {
            if (transaction is null)
            {
                connection.Dispose();
            }
        }
    }
}
