using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using SyslogWeb.Models;

namespace SyslogWeb.Pages
{
    public class IndexModel : PageModel
    {
        private readonly MongoDBConfig _mongoDb;
        private readonly ILogger<IndexModel> _logger;

        [BindProperty(SupportsGet = true, Name = "q")]
        public string Query { get; set; }
        
        public string QueryJson { get; set; }

        public long ParseTime { get; set; }

        public long FetchTime { get; set; }

        [BindProperty(SupportsGet = true, Name = "date")]
        public DateTime? Date { get; set; }

        public IEnumerable<SyslogFacility> SelectedFacilities { get; set; } 

        public IEnumerable<SyslogSeverity> SelectedSeverities { get; set; } 

        public IEnumerable<string> SelectedHosts { get; set; } 

        public IEnumerable<string> SelectedPrograms { get; set; } 

        public IEnumerable<SyslogEntry> LogEntries { get; set; }

        public IEnumerable<MongoResultRow> LogEntryModels
        {
            get
            {
                foreach (var x in LogEntries)
                {
                    yield return new MongoResultRow
                        {
                            Date = x.Date,
                            Facility = x.Facility,
                            Host = x.Host,
                            Id = x.Id,
                            Pid = x.Pid,
                            Program = x.Program,
                            Seq = x.Seq,
                            Severity = x.Severity,
                            Text = x.Text,
                            HostAsLink = !SelectedHosts.Contains(x.Host),
                            ProgramAsLink = !String.IsNullOrEmpty(x.Program) && !SelectedPrograms.Contains(x.Program),
                            Query = Query,
                            QueryDate = Date
                        };
                }
            }
        }
        
        public ObjectId ObjectId
        {
            get
            {
                if (Date.HasValue) return ObjectId.Empty; //Disable WS for Date filtered
                return LogEntries.Select(x=>x.Id).DefaultIfEmpty(ObjectId.Empty).Max();
            }
        }

        public DateTime? MinDate
        {
            get
            {
                return LogEntries.Select(x => (DateTime?)x.Date.LocalDateTime).DefaultIfEmpty(null).Min();
            }
        }

        public IndexModel(MongoDBConfig mongoDb, ILogger<IndexModel> logger)
        {
            _mongoDb = mongoDb;
            _logger = logger;
        }

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            var coll = _mongoDb.SyslogCollection;

            var sw = new Stopwatch();
            sw.Start();
            var parser = new QueryParser();
            var query = parser.Parse(Query, Date);
            Query = parser.QueryString;
            SelectedFacilities = parser.Facilities;
            SelectedSeverities = parser.Severities;
            SelectedPrograms = parser.Programs;
            SelectedHosts = parser.Hosts;

            sw.Stop();
            ParseTime = sw.ElapsedMilliseconds;
            var cursor = coll.Find(query);
            cursor.Sort(new SortDefinitionBuilder<SyslogEntry>().Descending(x=>x.Id));
            var result = cursor;

            sw.Restart();
            LogEntries = await result.Limit(100).ToListAsync(cancellationToken);
            sw.Stop();
            FetchTime = sw.ElapsedMilliseconds;
            
            if (query != null)
            {
                var jsonSettings = new JsonWriterSettings
                {
                    Indent = true,
                    OutputMode = JsonOutputMode.Strict
                };

                QueryJson = query.Render(BsonSerializer.SerializerRegistry.GetSerializer<SyslogEntry>(), BsonSerializer.SerializerRegistry).ToJson(jsonSettings);
            }
            sw.Stop();
        }
    }
}
