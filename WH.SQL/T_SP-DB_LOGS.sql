IF OBJECT_ID('[wh].[DB_LOGS]', 'U') IS NOT NULL
DROP TABLE [wh].[DB_LOGS]
GO
CREATE TABLE [wh].[DB_LOGS]
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
    INSERT INTO [wh].[DB_LOGS]
        ([l_DateTime], [l_ErrorNumber], [l_Severity], [l_State], [l_Procedure], [l_Line], [l_Message])
    VALUES
        (GETDATE(),@ErrorNumber,@Severity,@State,@Procedure,@Line,@Message);
END
GO