using System.Data;
using Microsoft.Data.SqlClient;
using NatarakiCarRental.Helpers;

namespace NatarakiCarRental.Data;

public sealed class DbConnectionFactory
{
    public DbConnectionFactory()
        : this(AppConstants.DefaultConnectionString)
    {
    }

    public DbConnectionFactory(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ConnectionString = connectionString;
    }

    public string ConnectionString { get; }

    public SqlConnection CreateConnection()
    {
        return new SqlConnection(ConnectionString);
    }

    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        return connection.State == ConnectionState.Open;
    }

    public bool TestConnection()
    {
        using SqlConnection connection = CreateConnection();
        connection.Open();

        return connection.State == ConnectionState.Open;
    }
}
