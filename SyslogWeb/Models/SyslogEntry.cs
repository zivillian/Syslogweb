using System;
using System.Text.RegularExpressions;
using MongoDB.Bson;

namespace SyslogWeb.Models
{
    public class SyslogEntry
    {
        public DateTimeOffset Date { get; set; }

        public SyslogSeverity Severity { get; set; }

        public SyslogFacility Facility { get; set; }

        public string Host { get; set; }

        public string Text { get; set; }

        public string Program { get; set; }

        public uint Pid { get; set; }

        public ulong Seq { get; set; }

        public ObjectId Id { get; set; }

        public string CssClass
        {
            get
            {
                switch (Severity)
                {
                    case SyslogSeverity.Emergency:
                    case SyslogSeverity.Alert:
                    case SyslogSeverity.Critical:
                    case SyslogSeverity.Error:
                    case SyslogSeverity.Crit:
                    case SyslogSeverity.Err:
                        return "table-danger";
                    case SyslogSeverity.Warning:
                        return "table-warning";
                    case SyslogSeverity.Notice:
                    case SyslogSeverity.Informational:
                        return "table-info";
                    case SyslogSeverity.Debug:
                        return "table-success";
                    default:
                        return String.Empty;
                }
            }
        }

        public override string ToString()
        {
            return String.Format("{0} {1} {2} {3} {4} {5} {6} {7}",
                                     Date.ToLocalTime(),
                                     Host,
                                     Facility,
                                     Severity,
                                     Pid,
                                     Program,
                                     Seq,
                                     Text);
        }
    }
}