using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Warehouse.Manager.Models;
using Microsoft.Extensions.Options;
using Warehouse.Manager.Utils;
using System.Data;
using System.Data.SqlClient;

namespace Warehouse.Manager.Services
{
  public interface IAuthenticationScopedService
  {
    Task<string> LoginUser(HttpContext context, Login login);
    Task<string> CreateUser(Models.User user);
  }

  public class AuthenticationScopedService : DbCommandBase, IAuthenticationScopedService
  {
    public string ClientIP { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public AuthenticationScopedService(IOptions<AppConfig> appSettings, ILoggingSingletonService loggingService, IHttpContextAccessor httpContextAccessor) : base(appSettings, loggingService, httpContextAccessor) { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private async Task<string> SignInUser(HttpContext context, Login login)
    {
      string res = string.Empty;
      try
      {
        UserIdentity userIdentity = await GetFirstResult<UserIdentity>(Queries.GetUser(login.Username));
        var principal = new ClaimsPrincipal(IdentityService.GetIdentity(userIdentity));
        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties()
        {
          IsPersistent = login.RememberLogin
        });
      }
      catch (Exception ex)
      {
        loggingService.Log(LogLevel.Error, nameof(SignInUser), ex, new Dictionary<string, object>
          {
              { "ClientIPAddress", ClientIP }
          });
      }
      return res;
    }

    public async Task<string> LoginUser(HttpContext context, Login login)
    {
      List<SqlParameter> prms = new() {
                new SqlParameter(Queries.Prms.User.USERNAME, login.Username),
                new SqlParameter(Queries.Prms.User.PASSWORD, login.Password),
                new SqlParameter { Direction = ParameterDirection.ReturnValue } 
      };
      try
      {
        await ExecuteNonQuery(Queries.SP.LOGIN, prms, CommandType.StoredProcedure);
        int res = (int)prms.Last().Value;

        if (res != 0)
          return DbResults.GetResult(nameof(LoginUser), res);
        await SignInUser(context, login);
      }
      catch (Exception ex)
      {
        loggingService.Log(LogLevel.Error, nameof(LoginUser), ex, new Dictionary<string, object>
          {
              { "ClientIPAddress", ClientIP = AppHelper.GetClientIPAddress(context) }
          });
        return DbResults.ConnectionError;
      }
      return string.Empty;
    }

    public async Task<string> CreateUser(Models.User user)
    {
      user.Phone = string.IsNullOrEmpty(user.Phone) ? string.Empty : user.Phone;
      List<SqlParameter> prms = new()
        {
        new SqlParameter(Queries.Prms.User.USERNAME, user.Username),
        new SqlParameter(Queries.Prms.User.PASSWORD, user.Password),
        new SqlParameter(Queries.Prms.User.EMAIL, user.EmailAddress),
        new SqlParameter(Queries.Prms.User.PHONE, user.Phone),
        new SqlParameter { Direction = ParameterDirection.ReturnValue } };
      try
      {
        await BeginTransaction();
        await ExecuteNonQuery(Queries.SP.CREATEUSER, prms, CommandType.StoredProcedure);
        await CommitTransaction();
        return DbResults.GetResult(nameof(CreateUser), (int)prms.Last().Value);
      }
      catch (Exception ex)
      {
        await RollbackTransaction();
        loggingService.Log(LogLevel.Error, nameof(CreateUser), ex, new Dictionary<string, object>
          {
              { "ClientIPAddress", ClientIP }
          });
        return DbResults.ConnectionError;
      }
    }
  }
}

