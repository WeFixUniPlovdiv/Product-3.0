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