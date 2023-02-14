using System;
using System.Data;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Warehouse.Manager.Utils
{
    public class AppHelper
    {
        public static string GetClientIPAddress(HttpContext httpContext) => (httpContext?.Connection?.RemoteIpAddress?.ToString()) ?? "000.000.000.000";

        public static List<T> ConvertDt<T>(DataTable o) =>
          JsonConvert.DeserializeObject<List<T>>(JsonConvert.SerializeObject(o));

        public static string ObjectToStringSafe(object obj, bool indented)
        {
            string result;
            try
            {
                if (indented)
                    result = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonConverter[] { new StringEnumConverter() });
                else
                    result = JsonConvert.SerializeObject(obj);
            }
            catch (Exception e)
            {
                result = ExceptionToStringSafe(e, indented);
            }
            return result;
        }

        public static string ExceptionToStringSafe(Exception e, bool indented)
        {
            string result;
            try
            {
                if (indented)
                    result = JsonConvert.SerializeObject(e, Formatting.Indented, new JsonConverter[] { new StringEnumConverter() });
                else
                    result = JsonConvert.SerializeObject(e);
            }
            catch (Exception)
            {
                result = string.Join(Environment.NewLine, new string[] { e.Message, e.StackTrace });
            }
            return result;
        }
    }
}

