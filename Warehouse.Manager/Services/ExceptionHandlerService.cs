using System;
using Newtonsoft.Json;
using Warehouse.Manager.Utils;

namespace Warehouse.Manager.Services
{
    public interface IExceptionHandlerService
    {
        Task HandleExceptionAsync(HttpContext context, Exception exception);
    }

    public class ExceptionHandlerService : IExceptionHandlerService
    {
        private readonly IHostEnvironment HostEnvironment;
        private readonly ILoggingSingletonService LoggingService;

        public ExceptionHandlerService(IHostEnvironment HostEnvironment, ILoggingSingletonService LoggingService)
        {
            this.HostEnvironment = HostEnvironment;
            this.LoggingService = LoggingService;
        }

        public async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
        {
            if (exception is not IAppException appException)
                appException = new UnknownException(exception.Message);

            var result = new Dictionary<string, object>() {
                  {"exceptionType",  appException.GetType().Name},
                  {nameof(appException.ClientTitle),  appException.ClientTitle},
                  {nameof(appException.ClientMessage),  appException.ClientMessage},
            };
            if (appException.Callback != ExceptionHelper.ExceptionCallback.NONE)
            {
                result.Add(nameof(appException.Callback), appException.Callback.ToString());
                if (appException.Args != null)
                    result.Add(nameof(appException.Args), appException.Args.ToDictionary(x => char.ToLowerInvariant(x.Key[0]) + x.Key.Substring(1), x => x.Value));
            }
            if (appException.Handler != ExceptionHelper.ExceptionHandler.NONE)
            {
                result.Add(nameof(appException.Handler), appException.Handler.ToString());
                if (appException.Args != null && !result.ContainsKey(nameof(appException.Args)))
                    result.Add(nameof(appException.Args), appException.Args.ToDictionary(x => char.ToLowerInvariant(x.Key[0]) + x.Key.Substring(1), x => x.Value));
            }

            if (HostEnvironment.IsDevelopment())
            {
                result.Add(nameof(appException.Message), appException.Message);
                result.Add(nameof(appException.StackTrace), appException.StackTrace);
            }

            if (appException is not ClientException)
                LoggingService.Log(Microsoft.Extensions.Logging.LogLevel.Error, "0x9000000", (Exception)appException);

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)appException.StatusCode;
            await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(result));
        }

    }
}

