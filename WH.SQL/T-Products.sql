IF OBJECT_ID('[wh].[T_Products]', 'U') IS NOT NULL
DROP TABLE [wh].[T_Products]
GO
CREATE TABLE [wh].[T_Products]
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
    FOREIGN KEY (TypeId) REFERENCES wh.T_ProductTypes(Id)
);