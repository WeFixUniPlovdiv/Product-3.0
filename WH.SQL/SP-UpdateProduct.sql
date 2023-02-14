CREATE OR ALTER PROCEDURE wh.SP_UpdateProduct
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
    IF EXISTS (SELECT TOP 1 [Id] FROM [wh].[T_Products] WHERE [Id] = @pID)
    BEGIN
        INSERT INTO @Prod SELECT * FROM [wh].[T_Products] WHERE [Id] = @pID;
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
        UPDATE [wh].[T_Products]
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