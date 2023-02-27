namespace Warehouse.Manager.Utils
{
    public class DbConnectionSettings
    {
        
        public Dictionary<string, string> Settings { get; set; }
        public string GetConnectionString() => string.Join(';', Settings.Select(x => string.Format("{0}={1}", x.Key, x.Value)));
        
    }
}

