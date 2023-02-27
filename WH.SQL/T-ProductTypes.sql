IF OBJECT_ID('[dbo].[T_ProductTypes]', 'U') IS NOT NULL
DROP TABLE [dbo].[T_ProductTypes]
GO
CREATE TABLE [dbo].[T_ProductTypes]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(255) UNIQUE NOT NULL,
    PRIMARY KEY (Id)
);