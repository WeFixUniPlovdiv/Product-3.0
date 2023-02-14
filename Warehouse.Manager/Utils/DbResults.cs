using System;
using Warehouse.Manager.Services;
namespace Warehouse.Manager.Utils
{
  public static class DbResults
  {
    public static string ConnectionError
    = "Възниикна проблем в комуникацията със сървъра. Моля обърнете се към администратор.";

    /// <summary>
    /// Get error based on the database result
    /// </summary>
    /// <returns>string</returns>
    public static string GetResult(string MethodName, int ResultCode)
    {
      string dbGeneralError = "Възникна проблем в базата данни. Моля обърнете се към администратор.";
      switch (MethodName)
      {
        case nameof(AuthenticationScopedService.LoginUser):
          {
            if (ResultCode == 0)
              return string.Empty;
            else if (ResultCode == 1)
              return "Грешно име или парола!";
          }
          break;
        case nameof(AuthenticationScopedService.CreateUser):
          {
            if (ResultCode == 1)
              return "Потребител с това име вече същесвува!";
            else if (ResultCode == 2)
              return "Потребител с този email вече същесвува!";
            else if (ResultCode == 3)
              return dbGeneralError;
          }
          break;
        case nameof(DbDataScopedService.AddProduct):
          {
            if (ResultCode == 0)
              return "Продукта е добавен успешно!";
            else if (ResultCode == 1)
              return "Количеството на продукта е обновено!";
            else if (ResultCode == 2)
              return dbGeneralError;
          }
          break;
        case nameof(DbDataScopedService.UpdateProduct):
          {
            if (ResultCode == 0)
              return "Продукта е обновен успешно!";
            else if (ResultCode == 1)
              return "Този продукт вече е изтрит или не съзществува!";
            else if (ResultCode == 2)
              return dbGeneralError;
          }
          break;
        case nameof(DbDataScopedService.DeleteProduct):
          {
            if (ResultCode == 0)
              return "Продукта е изтрит успешно!";
            else if (ResultCode == 1)
              return "Този продукт вече е изтрит или не съзществува!";
            else if (ResultCode == 2)
              return dbGeneralError;
          }
          break;
        case nameof(DbDataScopedService.AddProductType):
          {
            if (ResultCode == 0)
              return "Категорията е добавена успешно.";
            else if (ResultCode == 1)
            {
              return "Тази категория вече съществува!";
            }
            else if (ResultCode == 2)
            {
              return dbGeneralError;
            }
          }
          break;
        default: return string.Empty;
      }
      return string.Empty;
    }
  }
}

