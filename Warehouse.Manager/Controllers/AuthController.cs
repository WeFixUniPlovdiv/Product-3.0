using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Manager.Models;
using Warehouse.Manager.Services;

namespace Warehouse.Manager.Controllers
{
  public class AuthController : Controller
  {
    private ILoggingSingletonService Logger;
    private IAuthenticationScopedService Authentication;

    public AuthController(ILoggingSingletonService logger, IAuthenticationScopedService auth)
    {
      Logger = logger;
      Authentication = auth;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Index(string returnUrl)
    {
      AuthVM vm = new() { Login = new(), User = new(), selected = "login" };
      vm.Login.ReturnUrl = returnUrl;
      return View(vm);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromForm] Login login)
    {
      AuthVM vm = new() { Login = login, User = new(), selected = "login" };
      if (!ModelState.IsValid)
        return View(nameof(Index), vm);

      string result = await Authentication.LoginUser(HttpContext, login);
      if (!string.IsNullOrWhiteSpace(result))
      {
        ViewBag.LoginError = result;
        return View(nameof(Index), vm);
      }
      return LocalRedirect("/");
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register([FromForm] User user)
    {
      AuthVM vm = new() { Login = new(), User = user, selected = "register" };
      if (!ModelState.IsValid)
        return View(nameof(Index), vm);

      string result = await Authentication.CreateUser(user);
      if (!string.IsNullOrWhiteSpace(result))
      {
        ViewBag.RegisterError = result;
        return View(nameof(Index), vm);
      }
      vm.selected = "login";
      vm.User = new();
      return View(nameof(Index), vm);
    }

    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Logout()
    {
      await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
      return LocalRedirect("/");
    }
  }
}
