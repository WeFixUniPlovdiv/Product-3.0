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