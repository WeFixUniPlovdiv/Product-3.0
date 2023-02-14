IF OBJECT_ID('[wh].[T_ProductTypes]', 'U') IS NOT NULL
DROP TABLE [wh].[T_ProductTypes]
GO
CREATE TABLE [wh].[T_ProductTypes]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(255) UNIQUE NOT NULL,
    PRIMARY KEY (Id)
);