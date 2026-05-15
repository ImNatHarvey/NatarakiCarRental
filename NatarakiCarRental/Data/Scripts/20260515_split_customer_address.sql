IF OBJECT_ID(N'dbo.Customers', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.Customers', N'Region') IS NULL
    BEGIN
        ALTER TABLE dbo.Customers ADD Region nvarchar(150) NULL;
    END;

    IF COL_LENGTH(N'dbo.Customers', N'Province') IS NULL
    BEGIN
        ALTER TABLE dbo.Customers ADD Province nvarchar(150) NULL;
    END;

    IF COL_LENGTH(N'dbo.Customers', N'City') IS NULL
    BEGIN
        ALTER TABLE dbo.Customers ADD City nvarchar(150) NULL;
    END;

    IF COL_LENGTH(N'dbo.Customers', N'Barangay') IS NULL
    BEGIN
        ALTER TABLE dbo.Customers ADD Barangay nvarchar(150) NULL;
    END;

    IF COL_LENGTH(N'dbo.Customers', N'StreetAddress') IS NULL
    BEGIN
        ALTER TABLE dbo.Customers ADD StreetAddress nvarchar(255) NULL;
    END;

    -- Legacy addresses cannot be split reliably without a manual cleanup pass.
    -- Preserve the original free-form value in StreetAddress for existing rows.
    IF COL_LENGTH(N'dbo.Customers', N'Address') IS NOT NULL
    BEGIN
        UPDATE dbo.Customers
        SET StreetAddress = Address
        WHERE StreetAddress IS NULL
          AND Address IS NOT NULL
          AND LEN(LTRIM(RTRIM(Address))) > 0;
    END;
END;

/*
Run this only after manually validating migrated customer rows:

ALTER TABLE dbo.Customers DROP COLUMN Address;
*/
