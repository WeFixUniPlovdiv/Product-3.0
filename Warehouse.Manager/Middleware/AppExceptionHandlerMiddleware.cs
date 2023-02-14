using Microsoft.AspNetCore.Http;
using Warehouse.Manager.Services;
using System;
using System.Threading.Tasks;

namespace Warehouse.Manager.Middleware
{
    public class AppExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        public AppExceptionHandlerMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext httpContext, IExceptionHandlerService exceptionHandlerService)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await exceptionHandlerService.HandleExceptionAsync(httpContext, ex);
            }
        }
    }
}

