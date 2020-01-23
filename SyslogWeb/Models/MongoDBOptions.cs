namespace SyslogWeb.Models
{
    public class MongoDBOptions
    {
        public string ConnectionString { get; set; }

        public string Database { get; set; }

        public string Collection { get; set; }
    }
}