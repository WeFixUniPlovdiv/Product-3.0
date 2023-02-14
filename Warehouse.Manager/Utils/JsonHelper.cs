using System.Data;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System;
using Newtonsoft.Json.Serialization;


namespace Warehouse.Manager.Utils
{
    public class JsonKeyValue
    {
        public string JKey { get; set; }
        public object JValue { get; set; }
    }

    public class LowercaseContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToLower();
        }
    }

    public static class JsonHelper
    {
        private static Dictionary<string, object> JsonKeyValueDictionary(string json)
        {
            var list = JsonConvert.DeserializeObject<List<JsonKeyValue>>(json);
            if (list == null)
                return null;
            return list.ToDictionary(p => p.JKey, p => p.JValue);
        }

        private static List<object> JsonValueList(string json)
        {
            var list = JsonConvert.DeserializeObject<List<JsonKeyValue>>(json);
            if (list == null)
                return null;
            return list.Select(x => x.JValue).ToList();
        }

        private static Dictionary<string, T> DataRowDictionary<T>(DataRow dataRow)
        {
            Dictionary<string, T> row = new Dictionary<string, T>();
            foreach (DataColumn col in dataRow.Table.Columns)
            {
                string Key = col.ColumnName;
                T Value = default;
                try
                {
                    Value = (T)dataRow[col];
                }
                catch (Exception e)
                {
                    throw ExceptionHelper.GetException(e, _e => new ParseException(_e.Message));
                }
                if (typeof(T) == typeof(object))
                {
                    if (col.ColumnName.EndsWith("_JSON_Object"))
                    {
                        Key = col.ColumnName.Replace("_JSON_Object", string.Empty);
                        Value = JsonConvert.DeserializeObject<T>(dataRow[col].ToString());
                    }
                    if (col.ColumnName.EndsWith("_JSON_KeyValue"))
                    {
                        Key = col.ColumnName.Replace("_JSON_KeyValue", string.Empty);
                        Value = (T)(object)JsonKeyValueDictionary(dataRow[col].ToString());
                    }
                    if (col.ColumnName.EndsWith("_JSON_Array"))
                    {
                        Key = col.ColumnName.Replace("_JSON_Array", string.Empty);
                        Value = (T)(object)JsonValueList(dataRow[col].ToString());
                    }
                    if (col.ColumnName.EndsWith("_JSON_INT_Array"))
                    {
                        Key = col.ColumnName.Replace("_JSON_INT_Array", string.Empty);
                        var TmpList = JsonValueList(dataRow[col].ToString());
                        if (TmpList != null)
                            Value = (T)(object)TmpList.Select(x => int.Parse(x.ToString())).ToList();
                    }
                }
                row.Add(Key, Value);
            }
            return row;
        }

        public static List<T> DataTableToList<T>(DataTable dt)
        {
            var list = new List<T>();
            foreach (DataRow dataRow in dt.Rows)
            {
                T Value = default;
                try
                {
                    if (typeof(T) == typeof(string))
                    {
                        Value = (T)(object)dataRow[0].ToString();
                    }
                    else if (typeof(T) == typeof(int))
                    {
                        Value = (T)(object)int.Parse(dataRow[0].ToString());
                    }
                    else if (typeof(T) == typeof(long))
                    {
                        Value = (T)(object)long.Parse(dataRow[0].ToString());
                    }
                    else
                    {
                        Value = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(DataRowDictionary<object>(dataRow)));
                    }
                }
                catch (Exception e)
                {
                    throw ExceptionHelper.GetException(e, _e => new ParseException(_e.Message));
                }
                list.Add(Value);
            }
            return list;
        }

        public static List<Dictionary<string, T>> DataTableToDictionaryList<T>(DataTable dt)
        {
            var list = new List<Dictionary<string, T>>();
            foreach (DataRow dataRow in dt.Rows)
                list.Add(DataRowDictionary<T>(dataRow));
            return list;
        }
    }

}

