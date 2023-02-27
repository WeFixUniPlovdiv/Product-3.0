Create or ALTER VIEW [dbo].[V_Products] AS
    SELECT 
        p.[Id] as pID,
        t.[Name] as pType,
        p.[Name] as pName,
        p.[Description] as pDesc,
        p.[ImageB64] as pImg,
        p.[BoughtPrice] as pBPrice,
        p.[SellPrice] as pSPrice,
        p.[Amount] as pAmount
    FROM [dbo].[T_Products] p
    INNER JOIN [dbo].[T_ProductTypes] t ON t.Id = p.TypeId