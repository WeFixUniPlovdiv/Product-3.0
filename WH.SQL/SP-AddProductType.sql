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