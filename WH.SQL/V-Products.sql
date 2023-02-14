Create or ALTER VIEW [wh].[V_Products] AS
    SELECT 
        p.[Id] as pID,
        t.[Name] as pType,
        p.[Name] as pName,
        p.[Description] as pDesc,
        p.[ImageB64] as pImg,
        p.[BoughtPrice] as pBPrice,
        p.[SellPrice] as pSPrice,
        p.[Amount] as pAmount
    FROM [wh].[T_Products] p
    INNER JOIN [wh].[T_ProductTypes] t ON t.Id = p.TypeId