-- T-DB_LOGS
IF OBJECT_ID('[dbo].[DB_LOGS]', 'U') IS NOT NULL
DROP TABLE [dbo].[DB_LOGS]
GO
CREATE TABLE [dbo].[DB_LOGS]
(
    l_ID INT IDENTITY PRIMARY KEY,
    l_DateTime DATETIME,
    l_ErrorNumber SMALLINT,
    l_Severity SMALLINT,
    l_State SMALLINT,
    l_Procedure NVARCHAR(128),
    l_Line SMALLINT,
    l_Message NVARCHAR(MAX)
);

-- SP_Log_DbError
GO
CREATE OR ALTER PROCEDURE [dbo].[SP_Log_DbError]
    @ErrorNumber SMALLINT,
    @Severity SMALLINT,
    @State SMALLINT,
    @Procedure NVARCHAR(128),
    @Line SMALLINT,
    @Message NVARCHAR(MAX)
AS
BEGIN
    INSERT INTO [dbo].[DB_LOGS]
        ([l_DateTime], [l_ErrorNumber], [l_Severity], [l_State], [l_Procedure], [l_Line], [l_Message])
    VALUES
        (GETDATE(),@ErrorNumber,@Severity,@State,@Procedure,@Line,@Message);
END
GO

-- T-Users
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[uID] [int] IDENTITY(1,1) NOT NULL,
	[uUsername] [nvarchar](15) NOT NULL,
	[uPasswordHash] [binary](64) NOT NULL,
	[uPwdSalt] [uniqueidentifier] NOT NULL,
	[uEmail] [nvarchar](320) NOT NULL,
	[uPhone] [nvarchar](30) NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [PK_User_uID] PRIMARY KEY CLUSTERED 
(
	[uID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
ALTER TABLE [dbo].[Users] ADD UNIQUE NONCLUSTERED 
(
	[uUsername] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
ALTER TABLE [dbo].[Users] ADD UNIQUE NONCLUSTERED 
(
	[uEmail] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

-- SP_CreateUser
CREATE OR ALTER PROCEDURE [dbo].[SP_CreateUser]
    @Username NVARCHAR(15),
    @Password NVARCHAR(20),
    @Email NVARCHAR(320),
    @Phone NVARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON
    DECLARE @salt UNIQUEIDENTIFIER = NEWID()
    DECLARE @passed INT;

    IF (SELECT TOP 1 [uID] FROM [dbo].[Users] WHERE [uUsername] = @Username) IS NOT NULL RETURN 1;
    IF (SELECT TOP 1 [uID] FROM [dbo].[Users] WHERE LOWER([uEmail]) = LOWER(@Email)) IS NOT NULL RETURN 2;

    BEGIN TRY
        INSERT INTO [dbo].[Users]
            ([uUsername], [uPasswordHash], [uPwdSalt], [uEmail], [uPhone])
        VALUES
            (@Username, HASHBYTES('SHA2_512', @Password+CAST(@salt AS NVARCHAR(36))), @salt, @Email,@Phone)
    END TRY
    BEGIN CATCH
        DECLARE 
        @ErrorNumber SMALLINT = ERROR_NUMBER(),
        @Severity SMALLINT = ERROR_SEVERITY(),
        @State SMALLINT = ERROR_STATE(),
        @Procedure NVARCHAR(128) = ERROR_PROCEDURE(),
        @Line SMALLINT = ERROR_LINE(),
        @Message NVARCHAR(MAX) = ERROR_MESSAGE()
        EXECUTE [dbo].[SP_Log_DbError] @ErrorNumber, @Severity, @State, @Procedure, @Line, @Message;
        RETURN 3;
    END CATCH
END
GO

-- SP_LoginVerify
CREATE OR ALTER PROCEDURE [dbo].[SP_LoginVerify]
    @username NVARCHAR(15),
    @password  NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON
    DECLARE @userID INT;
    IF (SELECT TOP 1 [uID] FROM [dbo].[Users] WHERE [uUsername] LIKE @username OR [uEmail] LIKE @username) IS NOT NULL
    BEGIN
        SET @userID = (SELECT [uID] FROM  [dbo].[Users] 
                        WHERE [uUsername] LIKE @username 
                           OR [uEmail] LIKE @username
                          AND [uPasswordHash] = HASHBYTES('SHA2_512',@password+CAST(uPwdSalt AS NVARCHAR(36)))
                    )
        IF(@userID IS NULL) RETURN 1;
    END
    ELSE RETURN 1;
END
GO

-- T-ProductTypes
IF OBJECT_ID('[dbo].[T_ProductTypes]', 'U') IS NOT NULL
DROP TABLE [dbo].[T_ProductTypes]
GO
CREATE TABLE [dbo].[T_ProductTypes]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(255) UNIQUE NOT NULL,
    PRIMARY KEY (Id)
);
GO

-- SP_AddProductType
CREATE OR ALTER PROCEDURE [dbo].[SP_AddProductType]
    @Name NVARCHAR(255)
AS
BEGIN
    IF EXISTS (SELECT TOP 1 [Id] FROM [dbo].[T_ProductTypes] WHERE [Name] LIKE @Name)
            RETURN 1;
    BEGIN TRY
        INSERT INTO [dbo].[T_ProductTypes] ([Name]) VALUES (@Name);
    END TRY
    BEGIN CATCH
        DECLARE 
            @ErrorNumber SMALLINT = ERROR_NUMBER(),
            @Severity SMALLINT = ERROR_SEVERITY(),
            @State SMALLINT = ERROR_STATE(),
            @Procedure NVARCHAR(128) = ERROR_PROCEDURE(),
            @Line SMALLINT = ERROR_LINE(),
            @Message NVARCHAR(MAX) = ERROR_MESSAGE()
        EXECUTE [dbo].[SP_Log_DbError] @ErrorNumber, @Severity, @State, @Procedure, @Line, @Message;
        RETURN 2;
    END CATCH
END
GO

-- T-Product
IF OBJECT_ID('[dbo].[T_Products]', 'U') IS NOT NULL
DROP TABLE [dbo].[T_Products]
GO
CREATE TABLE [dbo].[T_Products]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [TypeId] INT NOT NULL,
    [Name] NVARCHAR(50) NOT NULL,
    [Description] NVARCHAR(2000),
    [ImageB64] VARBINARY(MAX),
    [BoughtPrice] FLOAT NOT NULL,
    [SellPrice] FLOAT NOT NULL,
    [Amount] INT NOT NULL
    PRIMARY KEY (Id),
    FOREIGN KEY (TypeId) REFERENCES dbo.T_ProductTypes(Id)
);
GO

-- SP_UpdateProduct
CREATE OR ALTER PROCEDURE dbo.SP_UpdateProduct
    @pID INT,
    @pTypeId INT = NULL,
    @pName NVARCHAR(50) = NULL,
    @pDescription NVARCHAR(2000) = null,
    @pImageB64 VARBINARY(MAX) = null,
    @pBoughtPrice FLOAT = NULL,
    @pSellPrice FLOAT = Null,
    @pAmount INT = NULL
AS
DECLARE
 @Prod as Table (pID INT , pTypeId INT,
            pName NVARCHAR(50), pDesc NVARCHAR(2000), pImg VARBINARY(MAX),
            pBPrice FLOAT, pSPrice FLOAT, pAmount INT)
BEGIN
--SET ANSI_WARNINGS OFF;
    IF EXISTS (SELECT TOP 1 [Id] FROM [dbo].[T_Products] WHERE [Id] = @pID)
    BEGIN
        INSERT INTO @Prod SELECT * FROM [dbo].[T_Products] WHERE [Id] = @pID;
    END
    BEGIN TRY
        IF (@pTypeId IS NOT NULL AND (SELECT pTypeId FROM @Prod) <> @pTypeId)
            UPDATE @Prod SET pTypeID = @pTypeID;
        IF (@pName IS NOT NULL AND (SELECT pName FROM @Prod) <> @pName)
            UPDATE @Prod SET pName = @pName;
        IF (@pDescription IS NOT NULL AND (SELECT pDesc FROM @Prod) <> @pDescription)
            UPDATE @Prod SET pDesc = @pDescription;
        IF (@pImageB64 IS NOT NULL AND (SELECT pImg FROM @Prod) <> @pImageB64)
            UPDATE @Prod SET pImg = @pImageB64;
        IF (@pBoughtPrice IS NOT NULL AND (SELECT pBPrice FROM @Prod) <> @pBoughtPrice)
            UPDATE @Prod SET pBPrice = @pBoughtPrice;
        IF (@pSellPrice IS NOT NULL AND (SELECT pSPrice FROM @Prod) <> @pSellPrice)
            UPDATE @Prod SET pSPrice = @pSellPrice;
        IF (@pAmount IS NOT NULL AND (SELECT pAmount FROM @Prod) <> @pAmount)
            UPDATE @Prod SET pAmount = @pAmount;
        UPDATE [dbo].[T_Products]
           SET [TypeId] = (SELECT pTypeId FROM @Prod),
               [Name] = (SELECT pName FROM @Prod),
               [Description] = (SELECT pDesc FROM @Prod),
               [ImageB64] = (SELECT pImg FROM @Prod),
               [BoughtPrice] = (SELECT pBPrice FROM @Prod),
               [SellPrice] = (SELECT pSPrice FROM @Prod),
               [Amount] = (SELECT pAmount FROM @Prod)
            WHERE [ID] = @pID
    END TRY
    BEGIN CATCH
        DECLARE 
            @ErrorNumber SMALLINT = ERROR_NUMBER(),
            @Severity SMALLINT = ERROR_SEVERITY(),
            @State SMALLINT = ERROR_STATE(),
            @Procedure NVARCHAR(128) = ERROR_PROCEDURE(),
            @Line SMALLINT = ERROR_LINE(),
            @Message NVARCHAR(MAX) = ERROR_MESSAGE()
        EXECUTE [dbo].[SP_Log_DbError] @ErrorNumber, @Severity, @State, @Procedure, @Line, @Message;
        RETURN 2;
    END CATCH
END
GO

-- SP_AddProduct
CREATE OR ALTER PROCEDURE [dbo].[SP_AddProduct]
    @pTypeId INT,
    @pName NVARCHAR(50),
    @pDescription NVARCHAR(2000),
    @pImageB64 VARBINARY(MAX),
    @pBoughtPrice FLOAT,
    @pSellPrice FLOAT,
    @pAmount INT
AS
DECLARE
    @pID INT = NULL
BEGIN
    SET @pID = (SELECT [Id] FROM [dbo].[T_Products] WHERE [Name] = @pName);
    BEGIN TRY
        IF @pID IS NOT NULL
        BEGIN
            SET @pAmount = @pAmount + (SELECT [Amount] FROM [dbo].[T_Products] WHERE [Id] = @pID);
            EXECUTE [dbo].[SP_UpdateProduct] @pID, @pAmount = @pAmount;
            RETURN 1;
        END
        INSERT INTO [dbo].[T_Products]
            ([TypeID],[Name],[Description],[ImageB64],[BoughtPrice],[SellPrice],[Amount]) 
        VALUES 
            (@pTypeID,@pName,@pDescription,@pImageB64,@pBoughtPrice,@pSellPrice,@pAmount);
    END TRY
    BEGIN CATCH
        DECLARE 
            @ErrorNumber SMALLINT = ERROR_NUMBER(),
            @Severity SMALLINT = ERROR_SEVERITY(),
            @State SMALLINT = ERROR_STATE(),
            @Procedure NVARCHAR(128) = ERROR_PROCEDURE(),
            @Line SMALLINT = ERROR_LINE(),
            @Message NVARCHAR(MAX) = ERROR_MESSAGE()
        EXECUTE [dbo].[SP_Log_DbError] @ErrorNumber, @Severity, @State, @Procedure, @Line, @Message;
        RETURN 2;
    END CATCH
END
GO

-- SP_DeleteProduct
CREATE OR ALTER PROCEDURE dbo.SP_DeleteProduct
    @pID INT
AS
BEGIN
    IF NOT EXISTS (SELECT TOP 1 [Id] FROM [dbo].[T_Products] WHERE [Id] = @pID)
        RETURN 1;
    BEGIN TRY
        DELETE FROM [dbo].[T_Products] WHERE [Id] = @pID;
    END TRY
    BEGIN CATCH
        DECLARE 
            @ErrorNumber SMALLINT = ERROR_NUMBER(),
            @Severity SMALLINT = ERROR_SEVERITY(),
            @State SMALLINT = ERROR_STATE(),
            @Procedure NVARCHAR(128) = ERROR_PROCEDURE(),
            @Line SMALLINT = ERROR_LINE(),
            @Message NVARCHAR(MAX) = ERROR_MESSAGE()
        EXECUTE [dbo].[SP_Log_DbError] @ErrorNumber, @Severity, @State, @Procedure, @Line, @Message;
        RETURN 2;
    END CATCH
END
GO

-- V-Products
Create or ALTER VIEW [dbo].[V_Products] AS
    SELECT 
        p.[Id] as pID,
        t.[Name] as pType,
        p.[Name] as pName,
        p.[Description] as pDesc,
        p.[ImageB64] as pImg,
        p.[BoughtPrice] as pBPrice,
        p.[SellPrice] as pSPrice,
        p.[Amount] as pAmount
    FROM [dbo].[T_Products] p
    INNER JOIN [dbo].[T_ProductTypes] t ON t.Id = p.TypeId