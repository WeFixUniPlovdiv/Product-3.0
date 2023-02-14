CREATE OR ALTER PROCEDURE [wh].[SP_CreateUser]
    @Username NVARCHAR(15),
    @Password NVARCHAR(20),
    @Email NVARCHAR(320),
    @Phone NVARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON
    DECLARE @salt UNIQUEIDENTIFIER = NEWID()
    DECLARE @passed INT;

    IF (SELECT TOP 1 [uID] FROM [wh].[Users] WHERE [uUsername] = @Username) IS NOT NULL RETURN 1;
    IF (SELECT TOP 1 [uID] FROM [wh].[Users] WHERE LOWER([uEmail]) = LOWER(@Email)) IS NOT NULL RETURN 2;

    BEGIN TRY
        INSERT INTO [wh].[Users]
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