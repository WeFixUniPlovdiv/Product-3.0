namespace Warehouse.Manager.Services
{
  static class Queries
  {
    public static string GetUser(string cridential)
    => @$"SELECT uID, uUsername, uEmail FROM {Table.USERS}
       WHERE uUsername = '{cridential}' OR uEmail = '{cridential}'";

    public static string GetProductTypes()
    => @$"SELECT *  FROM {Table.PRODUCT_TYPES}";

    public static string GetProduct(int id)
    => $"SELECT * FROM {View.PRODUCTS} WHERE {Prms.Product.V_ID} = {id}";

    public static string GetProducts(Dictionary<string, string?> wClause)
    {
      string stmt = $"SELECT * FROM {View.PRODUCTS} WHERE 1 = 1";

      if (wClause.Any())
      {
        var en = wClause.GetEnumerator();
        while (en.MoveNext())
        {
          stmt += string.Format(" AND {0} LIKE N'{1}'", en.Current.Key, en.Current.Value);
        }
      }
      return stmt;
    }
    public static class SP
    {
      public const string LOGIN = "SP_LoginVerify";
      public const string CREATEUSER = "SP_CreateUser";
      public const string ADD_PRODUCT_TYPE = "SP_AddProductType";
      public const string ADD_PRODUCT = "SP_AddProduct";
      public const string UPDATE_PRODUCT = "SP_UpdateProduct";
      public const string DELETE_PRODUCT = "SP_DeleteProduct";
    }

    public static class Table
    {
      public const string USERS = "Users";
      public const string PRODUCT_TYPES = "T_ProductTypes";
    }
    public static class View
    {
      public const string PRODUCTS = "V_Products";
    }
    public static class Prms
    {
      public static class User
      {
        public const string USERNAME = "@username";
        public const string PASSWORD = "@password";
        public const string EMAIL = "@email";
        public const string PHONE = "@phone";
      }
      public static class Product
      {
        public const string ID = "@pID";
        public const string TYPEID = "@pTypeId";
        public const string NAME = "@pName";
        public const string DESC = "@pDescription";
        public const string ImgB64 = "@pImageB64";
        public const string BPRICE = "@pBoughtPrice";
        public const string SPRICE = "@pSellPrice";
        public const string AMOUNT = "@pAmount";

        public const string V_ID = "pID";
        public const string V_TYPEID = "pType";
        public const string V_NAME = "pName";
        public const string V_DESC = "pDesc";
        public const string V_ImgB64 = "pImg";
        public const string V_BPRICE = "pBPrice";
        public const string V_SPRICE = "pSPrice";
        public const string V_AMOUNT = "pAmount";
      }
      public static class ProdType
      {
        public const string NAME = "@Name";
      }

    }
  }
}
