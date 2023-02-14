using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Warehouse.Manager.Models;
using static Warehouse.Manager.Services.Queries.Prms;

namespace Warehouse.Manager.Services
{
  public static class IdentityService
  {
    public static ClaimsIdentity GetIdentity(UserIdentity userIdentity)
    {
      var claims = new List<Claim>() {
	    new Claim(ClaimTypes.NameIdentifier,Convert.ToString(userIdentity.uID)),
        new Claim(ClaimTypes.Name, userIdentity.uUsername),
        new Claim(ClaimTypes.Email, userIdentity.uEmail)
        };
      return new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    }
  }
}
