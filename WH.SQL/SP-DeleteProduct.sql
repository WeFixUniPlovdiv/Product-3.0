CREATE OR ALTER PROCEDURE wh.SP_DeleteProduct
    @pID INT
AS
BEGIN
    IF NOT EXISTS (SELECT TOP 1 [Id] FROM [wh].[T_Products] WHERE [Id] = @pID)
        RETURN 1;
    BEGIN TRY
        DELETE FROM [wh].[T_Products] WHERE [Id] = @pID;
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