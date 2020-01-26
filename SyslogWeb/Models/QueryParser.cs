using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Driver;
using SyslogWeb.Extensions;

namespace SyslogWeb.Models
{
    public class QueryParser
    {
        public QueryParser()
        {
            Facilities = new SyslogFacility[0];
            Severities = new SyslogSeverity[0];
            Programs = new string[0];
            Hosts = new string[0];
        }

        public string[] TextTerms { get; private set; }

        public string[] Hosts { get; private set; }

        public string[] Programs { get; private set; }

        public string QueryString { get; private set; }

        public IList<SyslogSeverity> Severities { get; private set; }

        public IList<SyslogFacility> Facilities { get; private set; }

        public FilterDefinition<SyslogEntry> Parse(string search, DateTimeOffset? maxdate = null)
        {
            QueryString = String.Empty;
            if (String.IsNullOrEmpty(search)) return ParseDate(maxdate, FilterDefinition<SyslogEntry>.Empty);

            var items = search.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var namedItems = items.Where(x => x.Contains(":"))
                                  .Where(x => !x.StartsWith(":"))
                                  .Where(x => !x.EndsWith(":"))
                                  .Select(x => x.Split(':'))
                                  .Where(x => x.Length == 2)
                                  .GroupBy(x => x[0], x => x[1])
                                  .ToArray();
            TextTerms = items.Where(x => !x.Contains(":") || x.StartsWith(":") || x.EndsWith(":")).ToArray();

            var result = FilterDefinition<SyslogEntry>.Empty;
            var queryParts = new List<string>();
            foreach (var item in namedItems)
            {
                switch (item.Key.ToLowerInvariant())
                {
                    case "host":
                        result = result.And(item.Or(x => x.Host));
                        queryParts.Add(String.Join(" ", item.Select(x => String.Format("host:{0}", x)).ToArray()));
                        Hosts = item.ToArray();
                        break;
                    case "program":
                        result = result.And(item.Or(x => x.Program));
                        queryParts.Add(String.Join(" ", item.Select(x => String.Format("program:{0}", x)).ToArray()));
                        Programs = item.ToArray();
                        break;
                    case "facility":
                        {
                            result = result.And(OrEnum(x => x.Facility, item, out var enumValues));
                            queryParts.Add(String.Join(" ", item.Select(x => String.Format("facility:{0}", x)).ToArray()));
                            Facilities = enumValues;
                            break;
                        }
                    case "severity":
                        {
                            var orQuery = OrEnum(x => x.Severity, item, out var enumValues);
                            if (enumValues.Contains(SyslogSeverity.Crit) && !enumValues.Contains(SyslogSeverity.Critical))
                            {
                                orQuery = orQuery.Or(Builders<SyslogEntry>.Filter.Eq(x => x.Severity, SyslogSeverity.Critical));
                            }
                            else if (!enumValues.Contains(SyslogSeverity.Crit) && enumValues.Contains(SyslogSeverity.Critical))
                            {
                                orQuery = orQuery.Or(Builders<SyslogEntry>.Filter.Eq(x => x.Severity, SyslogSeverity.Crit));
                            }
                            if (enumValues.Contains(SyslogSeverity.Err) && !enumValues.Contains(SyslogSeverity.Error))
                            {
                                orQuery = orQuery.Or(Builders<SyslogEntry>.Filter.Eq(x => x.Severity, SyslogSeverity.Error));
                            }
                            else if (!enumValues.Contains(SyslogSeverity.Err) && enumValues.Contains(SyslogSeverity.Error))
                            {
                                orQuery = orQuery.Or(Builders<SyslogEntry>.Filter.Eq(x => x.Severity, SyslogSeverity.Err));
                            }
                            result = result.And(orQuery);
                            queryParts.Add(String.Join(" ", item.Select(x => String.Format("severity:{0}", x)).ToArray()));
                            Severities = enumValues;
                            break;
                        }
                    default:
                        continue;
                }
            }
            if (TextTerms.Any())
            {
                var searchText = String.Join(" ", TextTerms);
                result = result.And(Builders<SyslogEntry>.Filter.Text(searchText));
                queryParts.Add(searchText);
            }
            QueryString = String.Join(" ", queryParts);
            return ParseDate(maxdate, result);
        }

        private FilterDefinition<SyslogEntry> ParseDate(DateTimeOffset? maxdate, FilterDefinition<SyslogEntry> result)
        {
            if (maxdate.HasValue)
            {
                result = result.And(Builders<SyslogEntry>.Filter.Lte(x => x.Date, maxdate.Value));
            }
            return result;
        }

        private static FilterDefinition<SyslogEntry> OrEnum<T>(Expression<Func<SyslogEntry, T>> property, IEnumerable<string> values, out List<T> enumValues)
            where T : struct
        {
            var v = values.ToArray();
            var matchEnums = v.Where(x => !x.StartsWith("!")).ParseEnum<T>().ToArray();
            var notMatchEnums = v.Where(x => x.StartsWith("!")).Select(x => x.Substring(1)).ParseEnum<T>().ToArray();
            var matchQuery = FilterDefinition<SyslogEntry>.Empty;
            if (matchEnums.Any())
            {
                matchQuery = Builders<SyslogEntry>.Filter.Or(matchEnums.Select(x => Builders<SyslogEntry>.Filter.Eq(property, x)));
            }

            var notMatchQuery = FilterDefinition<SyslogEntry>.Empty;
            if (notMatchEnums.Any())
            {
                notMatchQuery = Builders<SyslogEntry>.Filter.And(notMatchEnums.Select(x => Builders<SyslogEntry>.Filter.Not(Builders<SyslogEntry>.Filter.Eq(property, x))));
            }
            enumValues = matchEnums.Concat(notMatchEnums).Distinct().ToList();
            return matchQuery.And(notMatchQuery);
        }

    }
}