using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Warehouse.Manager.Utils;
using System.Text;

namespace Warehouse.Manager.Services
{
    public interface ILoggingSingletonService
    {
        void Log(LogLevel LogLevel, string Code, string Message);
        void Log(LogLevel LogLevel, string Code, Exception Exception);
        void Log(LogLevel LogLevel, string Code, Exception Exception, Dictionary<string, object> Parameters);
        void Log(string Filename, LogLevel LogLevel, string Code, string Message, Exception Exception, Dictionary<string, object> Parameters);
        void DeviceLog(string message, Dictionary<string, object> parameters);
    }

    public class LoggingSingletonService : ILoggingSingletonService
    {
        private AppConfig appSettings;
        private readonly IHttpContextAccessor httpContextAccessor;
        private string GetLogFileName(string baseName) => $"{baseName}.log";
        private string GetClientIPAddress(HttpContext httpContext) => (httpContext?.Connection?.RemoteIpAddress?.ToString()) ?? "000.000.000.000";
        private string GetClientRequest(HttpContext httpContext)
        {
            var request = httpContext?.Request;
            if (request == null)
                return null;

            // Read request body as string
            // https://stackoverflow.com/a/40994711/3623177
            var BodyStr = "";
            try
            {
                // Allows using several time the stream in ASP.Net Core
                HttpRequestRewindExtensions.EnableBuffering(request);
                //request.EnableRewind();
                // Arguments: Stream, Encoding, detect encoding, buffer size 
                // AND, the most important: keep stream opened
                using (StreamReader reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
                    BodyStr = reader.ReadToEnd();
                // Rewind, so the core is not lost when it looks the body for the request
                request.Body.Position = 0;
            }
            catch (Exception) { }

            Dictionary<string, string> Form = null;
            try
            {
                Form = request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());
            }
            catch (Exception) { }

            return AppHelper.ObjectToStringSafe(new
            {
                request.Method,
                Url = Microsoft.AspNetCore.Http.Extensions.UriHelper.GetDisplayUrl(request),
                request.Path,
                request.Query,
                Body = BodyStr,
                Form,
                request.Headers
            }, true);
        }

        public LoggingSingletonService(IOptionsMonitor<AppConfig> appSettingsMonitor, IHttpContextAccessor httpContextAccessor)
        {
            appSettings = appSettingsMonitor.CurrentValue;
            appSettingsMonitor.OnChange(config => appSettings = config);
            this.httpContextAccessor = httpContextAccessor;
        }

        public void Log(LogLevel logLevel, string code, string message) =>
          Log(logLevel.ToString(), logLevel, code, message, null, null);
        public void Log(LogLevel logLevel, string code, Exception e) =>
          Log(logLevel.ToString(), logLevel, code, null, e, null);
        public void Log(LogLevel logLevel, string code, Exception e, Dictionary<string, object> parameters) =>
          Log(logLevel.ToString(), logLevel, code, null, e, parameters);
        public void Log(string filename, LogLevel logLevel, string code,
          string message,
          Exception exception,
          Dictionary<string, object> parameters
          )
      {
            var log_dir = appSettings.ErrorLogsDir;
            if (!Directory.Exists(log_dir))
                Directory.CreateDirectory(log_dir);

            log_dir = Path.Combine(log_dir, DateTime.Now.ToString("yyyy-MM-dd"));
            if (!Directory.Exists(log_dir))
                Directory.CreateDirectory(log_dir);

            using StreamWriter streamWriter = new StreamWriter(Path.Combine(log_dir, GetLogFileName(filename)), true);
            streamWriter.WriteLine("Time: " + DateTime.Now.ToString("HH:mm:ss"));
            streamWriter.WriteLine("LogLevel: " + logLevel);
            streamWriter.WriteLine("Code: " + code ?? "9x9999999");

            // Optional -->
            if (!string.IsNullOrEmpty(message))
                streamWriter.WriteLine("Message: " + message);
            if (exception != null)
                streamWriter.WriteLine("Exception: " + AppHelper.ExceptionToStringSafe(exception, true));
            if (parameters?.Count > 0)
                streamWriter.WriteLine("Parameters: " + AppHelper.ObjectToStringSafe(parameters, true));

            streamWriter.WriteLine("IPAddress: " + GetClientIPAddress(httpContextAccessor.HttpContext));
            streamWriter.WriteLine("Request: " + GetClientRequest(httpContextAccessor.HttpContext));
            streamWriter.WriteLine("-------------------------------------------------");
            streamWriter.WriteLine("");
            streamWriter.Close();
        }

        public void DeviceLog(string message, Dictionary<string, object> parameters) =>
            Log("DeviceLog", LogLevel.Error, "1x1111111", message, null, parameters);
    }
}


