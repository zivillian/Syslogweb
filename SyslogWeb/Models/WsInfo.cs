using MongoDB.Bson;
using Newtonsoft.Json;

namespace SyslogWeb.Models
{
    public class WsInfo
    {
        public string Id { get; set; }
    
        [JsonIgnore]
        public ObjectId ObjectId
        {
            get { return ObjectId.Parse(Id); }
        }
    
        public string Search { get; set; }
    }
}