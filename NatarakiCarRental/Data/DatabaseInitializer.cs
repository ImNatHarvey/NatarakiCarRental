using Microsoft.Data.SqlClient;
using NatarakiCarRental.Helpers;

namespace NatarakiCarRental.Data;

public static class DatabaseInitializer
{
    private const string DefaultOwnerUsername = "NatarakiCar";
    private const string DefaultOwnerPassword = "Nataraki2026";

    public static void Initialize()
    {
        CreateDatabaseIfMissing();
        CreateTablesIfMissing();
        SeedRoles();
        SeedDefaultDemoOwner();
    }

    private static void CreateDatabaseIfMissing()
    {
        const string sql = """
            IF NOT EXISTS (
                SELECT 1
                FROM sys.databases
                WHERE name = N'NatarakiCarRentalDb'
            )
            BEGIN
                CREATE DATABASE NatarakiCarRentalDb;
            END;
            """;

        using SqlConnection connection = new(AppConstants.MasterConnectionString);
        connection.Open();

        using SqlCommand command = new(sql, connection);
        command.ExecuteNonQuery();
    }

    private static void CreateTablesIfMissing()
    {
        ExecuteDatabaseCommand("""
            IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Roles
                (
                    RoleId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    RoleName nvarchar(50) NOT NULL UNIQUE,
                    Description nvarchar(255) NULL,
                    CreatedAt datetime2 NOT NULL DEFAULT sysdatetime()
                );
            END;
            """);

        ExecuteDatabaseCommand("""
            IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Users
                (
                    UserId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    RoleId int NOT NULL,
                    Username nvarchar(50) NOT NULL UNIQUE,
                    PasswordHash nvarchar(255) NOT NULL,
                    FirstName nvarchar(100) NOT NULL,
                    LastName nvarchar(100) NOT NULL,
                    Email nvarchar(150) NULL,
                    PhoneNumber nvarchar(30) NULL,
                    IsActive bit NOT NULL DEFAULT 1,
                    IsArchived bit NOT NULL DEFAULT 0,
                    CreatedAt datetime2 NOT NULL DEFAULT sysdatetime(),
                    UpdatedAt datetime2 NULL,
                    ArchivedAt datetime2 NULL,
                    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(RoleId)
                );
            END;
            """);

        ExecuteDatabaseCommand("""
            IF OBJECT_ID(N'dbo.Cars', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Cars
                (
                    CarId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    CarName nvarchar(100) NOT NULL,
                    Brand nvarchar(100) NULL,
                    Model nvarchar(100) NOT NULL,
                    PlateNumber nvarchar(20) NOT NULL UNIQUE,
                    [Year] int NULL,
                    Color nvarchar(50) NULL,
                    Transmission nvarchar(50) NULL,
                    FuelType nvarchar(50) NULL,
                    SeatingCapacity int NULL,
                    RatePerDay decimal(18,2) NOT NULL,
                    Status nvarchar(30) NOT NULL DEFAULT N'Available',
                    CodingDay nvarchar(30) NULL,
                    Mileage int NULL,
                    RegistrationExpirationDate date NULL,
                    InsuranceExpirationDate date NULL,
                    ImagePath nvarchar(500) NULL,
                    OrCrPath nvarchar(500) NULL,
                    IsArchived bit NOT NULL DEFAULT 0,
                    CreatedAt datetime2 NOT NULL DEFAULT sysdatetime(),
                    UpdatedAt datetime2 NULL,
                    ArchivedAt datetime2 NULL
                );
            END;
            """);

        ExecuteDatabaseCommand("""
            IF OBJECT_ID(N'dbo.Cars', N'U') IS NOT NULL
            BEGIN
                IF COL_LENGTH(N'dbo.Cars', N'CodingDay') IS NULL
                BEGIN
                    ALTER TABLE dbo.Cars ADD CodingDay nvarchar(30) NULL;
                END;

                IF COL_LENGTH(N'dbo.Cars', N'Mileage') IS NULL
                BEGIN
                    ALTER TABLE dbo.Cars ADD Mileage int NULL;
                END;

                IF COL_LENGTH(N'dbo.Cars', N'RegistrationExpirationDate') IS NULL
                BEGIN
                    ALTER TABLE dbo.Cars ADD RegistrationExpirationDate date NULL;
                END;

                IF COL_LENGTH(N'dbo.Cars', N'InsuranceExpirationDate') IS NULL
                BEGIN
                    ALTER TABLE dbo.Cars ADD InsuranceExpirationDate date NULL;
                END;
            END;
            """);

        ExecuteDatabaseCommand("""
            IF OBJECT_ID(N'dbo.ActivityLogs', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.ActivityLogs
                (
                    ActivityLogId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    UserId int NULL,
                    ActionType nvarchar(50) NOT NULL,
                    EntityName nvarchar(100) NULL,
                    EntityId int NULL,
                    Description nvarchar(500) NOT NULL,
                    CreatedAt datetime2 NOT NULL DEFAULT sysdatetime(),
                    CONSTRAINT FK_ActivityLogs_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId)
                );
            END;
            """);
    }

    private static void SeedRoles()
    {
        InsertRoleIfMissing("Owner", "Full system owner access");
        InsertRoleIfMissing("Admin", "Administrative access");
        InsertRoleIfMissing("Manager", "Manages daily operations and reports");
        InsertRoleIfMissing("Agent", "Handles bookings and customer transactions");
        InsertRoleIfMissing("Staff", "Basic staff access");
    }

    private static void InsertRoleIfMissing(string roleName, string description)
    {
        const string sql = """
            IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = @RoleName)
            BEGIN
                INSERT INTO dbo.Roles (RoleName, Description)
                VALUES (@RoleName, @Description);
            END;
            """;

        using SqlConnection connection = new(AppConstants.DefaultConnectionString);
        connection.Open();

        using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@RoleName", roleName);
        command.Parameters.AddWithValue("@Description", description);
        command.ExecuteNonQuery();
    }

    private static void SeedDefaultDemoOwner()
    {
        const string sql = """
            IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = @Username)
            BEGIN
                INSERT INTO dbo.Users
                (
                    RoleId,
                    Username,
                    PasswordHash,
                    FirstName,
                    LastName,
                    Email,
                    PhoneNumber
                )
                SELECT
                    RoleId,
                    @Username,
                    @PasswordHash,
                    @FirstName,
                    @LastName,
                    NULL,
                    NULL
                FROM dbo.Roles
                WHERE RoleName = N'Owner';
            END;
            """;

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(DefaultOwnerPassword);

        using SqlConnection connection = new(AppConstants.DefaultConnectionString);
        connection.Open();

        using SqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@Username", DefaultOwnerUsername);
        command.Parameters.AddWithValue("@PasswordHash", passwordHash);
        command.Parameters.AddWithValue("@FirstName", "System");
        command.Parameters.AddWithValue("@LastName", "Owner");
        command.ExecuteNonQuery();
    }

    private static void ExecuteDatabaseCommand(string sql)
    {
        using SqlConnection connection = new(AppConstants.DefaultConnectionString);
        connection.Open();

        using SqlCommand command = new(sql, connection);
        command.ExecuteNonQuery();
    }
}
