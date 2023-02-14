using Microsoft.AspNetCore.Authentication.Cookies;
using Warehouse.Manager.Services;
using Warehouse.Manager.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();

var appSettingsSection = builder.Configuration.GetSection("AppConfig");
var appSettings = appSettingsSection.Get<AppConfig>();
builder.Services.Configure<AppConfig>(appSettingsSection);

builder.Services.AddSingleton<ILoggingSingletonService, LoggingSingletonService>();
builder.Services.AddScoped<IAuthenticationScopedService, AuthenticationScopedService>();
builder.Services.AddScoped<IDbDataScopedService, DbDataScopedService>();
builder.Services.AddTransient<IExceptionHandlerService, ExceptionHandlerService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                  options.Cookie.Name = "active_session";
                  options.LoginPath = "/Auth";
                  options.ExpireTimeSpan = new TimeSpan(0, 30, 0);
                });
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Home/Error");
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();
app.UseAuthentication();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Warehouse}/{action=Index}/{id?}");

app.Run();
