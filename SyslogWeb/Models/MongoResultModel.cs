using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MongoDB.Bson;

namespace SyslogWeb.Models
{
    public class MongoResultModel
    {
        public MongoResultModel()
        {
            LogEntries = Enumerable.Empty<SyslogEntry>();
            SelectedFacilities = Enumerable.Empty<SyslogFacility>();
            SelectedSeverities = Enumerable.Empty<SyslogSeverity>();
            SelectedHosts = Enumerable.Empty<string>();
            SelectedPrograms = Enumerable.Empty<string>();
        }

        public long Total { get; set; }

        public string Query { get; set; }
        
        public string QueryJson { get; set; }

        public long ParseTime { get; set; }

        public long FetchTime { get; set; }

        public long CountTime { get; set; }

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

        public Stopwatch RenderTime { get; set; }

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
    }

    public class MongoResultRow:SyslogEntry
    {
        public bool HostAsLink { get; set; }

        public bool ProgramAsLink { get; set; }

        public string Query { get; set; }

        public DateTime? QueryDate { get; set; }
    }
}