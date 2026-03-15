IF DB_ID('TravelTime') IS NULL
BEGIN
    CREATE DATABASE TravelTime;
END
GO

USE TravelTime;
GO

IF OBJECT_ID('dbo.Payments', 'U') IS NOT NULL DROP TABLE dbo.Payments;
IF OBJECT_ID('dbo.Bookings', 'U') IS NOT NULL DROP TABLE dbo.Bookings;
IF OBJECT_ID('dbo.Attractions', 'U') IS NOT NULL DROP TABLE dbo.Attractions;
IF OBJECT_ID('dbo.Clients', 'U') IS NOT NULL DROP TABLE dbo.Clients;
GO

CREATE TABLE Clients (
    ClientID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    Email VARCHAR(255) UNIQUE NOT NULL,
    Phone VARCHAR(20),
    Country VARCHAR(100),
    DateCreated DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE Attractions (
    AttractionID INT IDENTITY(1,1) PRIMARY KEY,
    AttractionName VARCHAR(200) NOT NULL,
    Location VARCHAR(200) NOT NULL,
    Description TEXT,
    OpeningHours VARCHAR(200),
    BasePrice DECIMAL(10,2) NOT NULL
);
GO

CREATE TABLE Bookings (
    BookingID INT IDENTITY(1,1) PRIMARY KEY,
    ClientID INT NOT NULL,
    AttractionID INT NOT NULL,
    BookingDate DATE NOT NULL,
    TourDate DATE NOT NULL,
    TotalAmount DECIMAL(10,2) NOT NULL,
    PaymentStatus VARCHAR(50) DEFAULT 'Pending',
    BookingStatus VARCHAR(50) DEFAULT 'Active',
    CONSTRAINT FK_Bookings_Clients FOREIGN KEY (ClientID) REFERENCES Clients(ClientID),
    CONSTRAINT FK_Bookings_Attractions FOREIGN KEY (AttractionID) REFERENCES Attractions(AttractionID)
);
GO

CREATE TABLE Payments (
    PaymentID INT IDENTITY(1,1) PRIMARY KEY,
    BookingID INT NOT NULL,
    PaymentDate DATETIME NOT NULL DEFAULT GETDATE(),
    Amount DECIMAL(10,2) NOT NULL,
    PaymentMethod VARCHAR(50),
    TransactionReference VARCHAR(200),
    CONSTRAINT FK_Payments_Bookings FOREIGN KEY (BookingID) REFERENCES Bookings(BookingID)
);
GO

IF OBJECT_ID('dbo.usp_Client_Add', 'P') IS NOT NULL DROP PROCEDURE dbo.usp_Client_Add;
IF OBJECT_ID('dbo.usp_Client_Update', 'P') IS NOT NULL DROP PROCEDURE dbo.usp_Client_Update;
IF OBJECT_ID('dbo.usp_Client_Delete', 'P') IS NOT NULL DROP PROCEDURE dbo.usp_Client_Delete;
IF OBJECT_ID('dbo.usp_Attraction_Add', 'P') IS NOT NULL DROP PROCEDURE dbo.usp_Attraction_Add;
IF OBJECT_ID('dbo.usp_Attraction_Update', 'P') IS NOT NULL DROP PROCEDURE dbo.usp_Attraction_Update;
GO

CREATE PROCEDURE dbo.usp_Client_Add
    @FirstName VARCHAR(100),
    @LastName VARCHAR(100),
    @Email VARCHAR(255),
    @Phone VARCHAR(20) = NULL,
    @Country VARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Clients (FirstName, LastName, Email, Phone, Country)
    VALUES (@FirstName, @LastName, @Email, @Phone, @Country);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS NewClientID;
END
GO

CREATE PROCEDURE dbo.usp_Client_Update
    @ClientID INT,
    @FirstName VARCHAR(100),
    @LastName VARCHAR(100),
    @Email VARCHAR(255),
    @Phone VARCHAR(20) = NULL,
    @Country VARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Clients
    SET FirstName = @FirstName,
        LastName = @LastName,
        Email = @Email,
        Phone = @Phone,
        Country = @Country
    WHERE ClientID = @ClientID;

    SELECT @@ROWCOUNT AS RowsUpdated;
END
GO

CREATE PROCEDURE dbo.usp_Client_Delete
    @ClientID INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM Clients
    WHERE ClientID = @ClientID;

    SELECT @@ROWCOUNT AS RowsDeleted;
END
GO

CREATE PROCEDURE dbo.usp_Attraction_Add
    @AttractionName VARCHAR(200),
    @Location VARCHAR(200),
    @Description TEXT = NULL,
    @OpeningHours VARCHAR(200) = NULL,
    @BasePrice DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Attractions (AttractionName, Location, Description, OpeningHours, BasePrice)
    VALUES (@AttractionName, @Location, @Description, @OpeningHours, @BasePrice);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS NewAttractionID;
END
GO

CREATE PROCEDURE dbo.usp_Attraction_Update
    @AttractionID INT,
    @AttractionName VARCHAR(200),
    @Location VARCHAR(200),
    @Description TEXT = NULL,
    @OpeningHours VARCHAR(200) = NULL,
    @BasePrice DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Attractions
    SET AttractionName = @AttractionName,
        Location = @Location,
        Description = @Description,
        OpeningHours = @OpeningHours,
        BasePrice = @BasePrice
    WHERE AttractionID = @AttractionID;

    SELECT @@ROWCOUNT AS RowsUpdated;
END
GO
