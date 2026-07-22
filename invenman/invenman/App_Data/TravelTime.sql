IF DB_ID(N'TravelTime') IS NULL
BEGIN
    CREATE DATABASE TravelTime;
END;
GO

USE TravelTime;
GO

IF OBJECT_ID(N'dbo.Clients', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Clients
    (
        ClientID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Clients PRIMARY KEY,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        Email NVARCHAR(255) NOT NULL,
        Phone NVARCHAR(30) NULL,
        Country NVARCHAR(100) NULL,
        DateCreated DATETIME2(0) NOT NULL
            CONSTRAINT DF_Clients_DateCreated DEFAULT SYSUTCDATETIME()
    );

    CREATE UNIQUE INDEX UX_Clients_Email ON dbo.Clients(Email);
END;
GO

IF OBJECT_ID(N'dbo.Attractions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Attractions
    (
        AttractionID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Attractions PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        Parish NVARCHAR(100) NOT NULL,
        Location NVARCHAR(200) NULL,
        Category NVARCHAR(100) NOT NULL,
        BasePrice DECIMAL(18,2) NOT NULL
            CONSTRAINT CK_Attractions_BasePrice CHECK (BasePrice >= 0),
        ChildPrice DECIMAL(18,2) NOT NULL
            CONSTRAINT DF_Attractions_ChildPrice DEFAULT 0
            CONSTRAINT CK_Attractions_ChildPrice CHECK (ChildPrice >= 0),
        OpenDays NVARCHAR(200) NULL,
        DurationMinutes INT NOT NULL
            CONSTRAINT DF_Attractions_DurationMinutes DEFAULT 180
            CONSTRAINT CK_Attractions_DurationMinutes CHECK (DurationMinutes BETWEEN 30 AND 1440),
        IsActive BIT NOT NULL
            CONSTRAINT DF_Attractions_IsActive DEFAULT 1
    );
END
ELSE
BEGIN
    IF COL_LENGTH(N'dbo.Attractions', N'Name') IS NULL
        ALTER TABLE dbo.Attractions ADD Name NVARCHAR(200) NULL;
    IF COL_LENGTH(N'dbo.Attractions', N'Parish') IS NULL
        ALTER TABLE dbo.Attractions ADD Parish NVARCHAR(100) NULL;
    IF COL_LENGTH(N'dbo.Attractions', N'Location') IS NULL
        ALTER TABLE dbo.Attractions ADD Location NVARCHAR(200) NULL;
    IF COL_LENGTH(N'dbo.Attractions', N'Category') IS NULL
        ALTER TABLE dbo.Attractions ADD Category NVARCHAR(100) NULL;
    IF COL_LENGTH(N'dbo.Attractions', N'ChildPrice') IS NULL
        ALTER TABLE dbo.Attractions ADD ChildPrice DECIMAL(18,2) NOT NULL
            CONSTRAINT DF_Attractions_ChildPrice_Migration DEFAULT 0;
    IF COL_LENGTH(N'dbo.Attractions', N'OpenDays') IS NULL
        ALTER TABLE dbo.Attractions ADD OpenDays NVARCHAR(200) NULL;
    IF COL_LENGTH(N'dbo.Attractions', N'DurationMinutes') IS NULL
        ALTER TABLE dbo.Attractions ADD DurationMinutes INT NOT NULL
            CONSTRAINT DF_Attractions_DurationMinutes_Migration DEFAULT 180;
    IF COL_LENGTH(N'dbo.Attractions', N'IsActive') IS NULL
        ALTER TABLE dbo.Attractions ADD IsActive BIT NOT NULL
            CONSTRAINT DF_Attractions_IsActive_Migration DEFAULT 1;

    IF COL_LENGTH(N'dbo.Attractions', N'AttractionName') IS NOT NULL
    BEGIN
        EXEC(N'UPDATE dbo.Attractions SET Name = COALESCE(Name, AttractionName);');
        EXEC(N'ALTER TABLE dbo.Attractions ALTER COLUMN AttractionName VARCHAR(200) NULL;');
    END;
    IF COL_LENGTH(N'dbo.Attractions', N'Location') IS NOT NULL
    BEGIN
        EXEC(N'UPDATE dbo.Attractions SET Parish = COALESCE(Parish, Location, N''Unknown'');');
        EXEC(N'ALTER TABLE dbo.Attractions ALTER COLUMN Location VARCHAR(200) NULL;');
    END;
    IF COL_LENGTH(N'dbo.Attractions', N'OpeningHours') IS NOT NULL
        EXEC(N'UPDATE dbo.Attractions SET OpenDays = COALESCE(OpenDays, OpeningHours);');

    EXEC(N'UPDATE dbo.Attractions
        SET Name = COALESCE(Name, N''Unnamed attraction''),
            Parish = COALESCE(Parish, N''Unknown''),
            Category = COALESCE(Category, N''Tour'');');
END;
GO

IF OBJECT_ID(N'dbo.Bookings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Bookings
    (
        BookingID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Bookings PRIMARY KEY,
        ClientID INT NOT NULL,
        AttractionID INT NOT NULL,
        BookingDate DATE NOT NULL CONSTRAINT DF_Bookings_BookingDate DEFAULT CAST(GETDATE() AS DATE),
        TourDate DATE NOT NULL,
        PartySize INT NOT NULL
            CONSTRAINT DF_Bookings_PartySize DEFAULT 1
            CONSTRAINT CK_Bookings_PartySize CHECK (PartySize > 0),
        TotalAmount DECIMAL(18,2) NOT NULL
            CONSTRAINT CK_Bookings_TotalAmount CHECK (TotalAmount >= 0),
        PaymentStatus NVARCHAR(50) NOT NULL
            CONSTRAINT DF_Bookings_PaymentStatus DEFAULT N'Pending',
        BookingStatus NVARCHAR(50) NOT NULL
            CONSTRAINT DF_Bookings_BookingStatus DEFAULT N'Active',
        TransportProvider NVARCHAR(150) NULL,
        PickupLocation NVARCHAR(200) NULL,
        PickupParish NVARCHAR(100) NULL,
        PickupDateTime DATETIME2(0) NULL,
        TransportNotes NVARCHAR(MAX) NULL,
        CONSTRAINT FK_Bookings_Clients FOREIGN KEY (ClientID) REFERENCES dbo.Clients(ClientID),
        CONSTRAINT FK_Bookings_Attractions FOREIGN KEY (AttractionID) REFERENCES dbo.Attractions(AttractionID)
    );
END
ELSE
BEGIN
    IF COL_LENGTH(N'dbo.Bookings', N'TransportProvider') IS NULL
        ALTER TABLE dbo.Bookings ADD TransportProvider NVARCHAR(150) NULL;
    IF COL_LENGTH(N'dbo.Bookings', N'PartySize') IS NULL
        ALTER TABLE dbo.Bookings ADD PartySize INT NOT NULL
            CONSTRAINT DF_Bookings_PartySize_Migration DEFAULT 1;
    IF COL_LENGTH(N'dbo.Bookings', N'PickupLocation') IS NULL
        ALTER TABLE dbo.Bookings ADD PickupLocation NVARCHAR(200) NULL;
    IF COL_LENGTH(N'dbo.Bookings', N'PickupParish') IS NULL
        ALTER TABLE dbo.Bookings ADD PickupParish NVARCHAR(100) NULL;
    IF COL_LENGTH(N'dbo.Bookings', N'PickupDateTime') IS NULL
        ALTER TABLE dbo.Bookings ADD PickupDateTime DATETIME2(0) NULL;
    IF COL_LENGTH(N'dbo.Bookings', N'TransportNotes') IS NULL
        ALTER TABLE dbo.Bookings ADD TransportNotes NVARCHAR(MAX) NULL;
END;
GO

IF OBJECT_ID(N'dbo.Vehicles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Vehicles
    (
        VehicleID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Vehicles PRIMARY KEY,
        VehicleCode NVARCHAR(40) NOT NULL,
        ProviderName NVARCHAR(150) NOT NULL,
        Capacity INT NOT NULL CONSTRAINT CK_Vehicles_Capacity CHECK (Capacity > 0),
        HomeParish NVARCHAR(100) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Vehicles_IsActive DEFAULT 1,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Vehicles_CreatedAt DEFAULT SYSUTCDATETIME()
    );

    CREATE UNIQUE INDEX UX_Vehicles_VehicleCode ON dbo.Vehicles(VehicleCode);
END;
GO

IF OBJECT_ID(N'dbo.Payments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Payments
    (
        PaymentID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Payments PRIMARY KEY,
        BookingID INT NOT NULL,
        PaymentDate DATETIME2(0) NOT NULL
            CONSTRAINT DF_Payments_PaymentDate DEFAULT SYSUTCDATETIME(),
        Amount DECIMAL(18,2) NOT NULL
            CONSTRAINT CK_Payments_Amount CHECK (Amount > 0),
        PaymentMethod NVARCHAR(50) NOT NULL,
        TransactionReference NVARCHAR(200) NULL,
        CurrencyCode CHAR(3) NOT NULL
            CONSTRAINT DF_Payments_CurrencyCode DEFAULT 'JMD',
        CONSTRAINT FK_Payments_Bookings FOREIGN KEY (BookingID) REFERENCES dbo.Bookings(BookingID)
    );
END
ELSE IF COL_LENGTH(N'dbo.Payments', N'CurrencyCode') IS NULL
BEGIN
    ALTER TABLE dbo.Payments ADD CurrencyCode CHAR(3) NOT NULL
        CONSTRAINT DF_Payments_CurrencyCode_Migration DEFAULT 'JMD';
END;
GO

IF OBJECT_ID(N'dbo.Refunds', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Refunds
    (
        RefundID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Refunds PRIMARY KEY,
        PaymentID INT NOT NULL,
        RefundAmount DECIMAL(18,2) NOT NULL
            CONSTRAINT CK_Refunds_Amount CHECK (RefundAmount > 0),
        Reason NVARCHAR(500) NULL,
        RefundDate DATETIME2(0) NOT NULL
            CONSTRAINT DF_Refunds_Date DEFAULT SYSUTCDATETIME(),
        ProcessedByUser NVARCHAR(100) NOT NULL,
        CONSTRAINT FK_Refunds_Payments FOREIGN KEY (PaymentID) REFERENCES dbo.Payments(PaymentID)
    );

    CREATE UNIQUE INDEX UX_Refunds_PaymentID ON dbo.Refunds(PaymentID);
END;
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        UserID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        Username NVARCHAR(100) NOT NULL,
        UserPassword VARCHAR(512) NOT NULL,
        RoleName NVARCHAR(50) NOT NULL
            CONSTRAINT CK_Users_Role CHECK (RoleName IN (N'Admin', N'Staff', N'Client')),
        IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT 1,
        Email NVARCHAR(255) NULL,
        FailedLoginCount INT NOT NULL CONSTRAINT DF_Users_FailedLoginCount DEFAULT 0,
        LockoutEndUtc DATETIME2(0) NULL,
        LastLoginAt DATETIME2(0) NULL,
        CreatedAt DATETIME2(0) NOT NULL
            CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME()
    );

    CREATE UNIQUE INDEX UX_Users_Username ON dbo.Users(Username);
END;
ELSE
BEGIN
    IF COL_LENGTH(N'dbo.Users', N'FailedLoginCount') IS NULL
        ALTER TABLE dbo.Users ADD FailedLoginCount INT NOT NULL
            CONSTRAINT DF_Users_FailedLoginCount_Migration DEFAULT 0;
    IF COL_LENGTH(N'dbo.Users', N'LockoutEndUtc') IS NULL
        ALTER TABLE dbo.Users ADD LockoutEndUtc DATETIME2(0) NULL;
    IF COL_LENGTH(N'dbo.Users', N'LastLoginAt') IS NULL
        ALTER TABLE dbo.Users ADD LastLoginAt DATETIME2(0) NULL;
END;
GO

IF OBJECT_ID(N'dbo.AuditLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLogs
    (
        AuditLogID BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditLogs PRIMARY KEY,
        LogDate DATETIME2(0) NOT NULL
            CONSTRAINT DF_AuditLogs_LogDate DEFAULT SYSUTCDATETIME(),
        Username NVARCHAR(100) NOT NULL,
        RoleName NVARCHAR(50) NOT NULL,
        ActionType NVARCHAR(100) NOT NULL,
        PageUrl NVARCHAR(500) NULL,
        Details NVARCHAR(MAX) NULL
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Bookings_TourDate' AND object_id = OBJECT_ID(N'dbo.Bookings'))
    CREATE INDEX IX_Bookings_TourDate ON dbo.Bookings(TourDate, BookingStatus);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Bookings_ClientID' AND object_id = OBJECT_ID(N'dbo.Bookings'))
    CREATE INDEX IX_Bookings_ClientID ON dbo.Bookings(ClientID, TourDate);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Bookings_TransportPlanning' AND object_id = OBJECT_ID(N'dbo.Bookings'))
    CREATE INDEX IX_Bookings_TransportPlanning
        ON dbo.Bookings(TourDate, BookingStatus, TransportProvider)
        INCLUDE (PartySize, AttractionID, PickupParish, PickupDateTime);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Payments_BookingID' AND object_id = OBJECT_ID(N'dbo.Payments'))
    CREATE INDEX IX_Payments_BookingID ON dbo.Payments(BookingID, PaymentDate);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AuditLogs_LogDate' AND object_id = OBJECT_ID(N'dbo.AuditLogs'))
    CREATE INDEX IX_AuditLogs_LogDate ON dbo.AuditLogs(LogDate DESC);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Attractions)
BEGIN
    INSERT INTO dbo.Attractions
        (Name, Description, Parish, Location, Category, BasePrice, ChildPrice, OpenDays, IsActive)
    VALUES
        (N'Dunns River Falls', N'Waterfall and guided climbing experience.', N'St. Ann', N'Ocho Rios', N'Adventure', 9500, 6500, N'Mon-Sun', 1),
        (N'Blue Mountain Trail', N'Guided mountain hiking experience.', N'St. Andrew', N'Blue Mountains', N'Hiking', 12000, 8000, N'Tue-Sun', 1),
        (N'Kingston Culture Tour', N'City history, music, and food tour.', N'Kingston', N'Kingston', N'Culture', 7000, 4500, N'Mon-Sat', 1);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Vehicles)
BEGIN
    INSERT INTO dbo.Vehicles (VehicleCode, ProviderName, Capacity, HomeParish, IsActive)
    VALUES
        (N'KNG-SHUTTLE-01', N'Island Shuttle Co.', 8, N'Kingston', 1),
        (N'MBJ-VAN-01', N'North Coast Transit', 12, N'St. James', 1),
        (N'OCJ-COACH-01', N'North Coast Transit', 24, N'St. Ann', 1);
END;
GO
