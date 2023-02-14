using System;
namespace Warehouse.Manager.Utils
{
    public class AppConfig
    {
        public string DbConnection { get; set; }
        public Dictionary<string, DbConnectionSettings> DbConnectionsStrings { get; set; }
        public string ErrorLogsDir { get; set; }
    }   
}

