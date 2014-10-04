using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
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

		protected IList<SyslogSeverity> Severities { get; private set; }

		protected IList<SyslogFacility> Facilities { get; private set; }

		public static IMongoQuery Parse(string search, MongoResultModel model)
		{
			var parser = new QueryParser();
			var result =  parser.Parse(search, model.Date);
			model.Query = parser.QueryString;
			model.SelectedFacilities = parser.Facilities;
			model.SelectedSeverities = parser.Severities;
			model.SelectedPrograms = parser.Programs;
			model.SelectedHosts = parser.Hosts;
			return result;
		}

		public IMongoQuery Parse(string search, DateTime? maxdate = null)
		{
			QueryString = String.Empty;
			if (String.IsNullOrEmpty(search)) return ParseDate(maxdate, Query.Null);

			var items = search.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			var namedItems = items.Where(x => x.Contains(":"))
								  .Where(x => !x.StartsWith(":"))
								  .Where(x => !x.EndsWith(":"))
								  .Select(x => x.Split(':'))
								  .Where(x => x.Length == 2)
								  .GroupBy(x => x[0], x => x[1])
								  .ToArray();
			TextTerms = items.Where(x => !x.Contains(":") || x.StartsWith(":") || x.EndsWith(":")).ToArray();

			IMongoQuery result = Query.Null;
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
							List<SyslogFacility> enumValues;
							result = result.And(OrEnum(x => x.Facility, item, out enumValues));
							queryParts.Add(String.Join(" ", item.Select(x => String.Format("facility:{0}", x)).ToArray()));
							Facilities = enumValues;
							break;
						}
					case "severity":
						{
							List<SyslogSeverity> enumValues;
							var orQuery = OrEnum(x => x.Severity, item, out enumValues);
							if (enumValues.Contains(SyslogSeverity.Crit) && !enumValues.Contains(SyslogSeverity.Critical))
							{
								orQuery = Query.Or(orQuery, Query<SyslogEntry>.EQ(x => x.Severity, SyslogSeverity.Critical));
							}
							else if (!enumValues.Contains(SyslogSeverity.Crit) && enumValues.Contains(SyslogSeverity.Critical))
							{
								orQuery = Query.Or(orQuery, Query<SyslogEntry>.EQ(x => x.Severity, SyslogSeverity.Crit));
							}
							if (enumValues.Contains(SyslogSeverity.Err) && !enumValues.Contains(SyslogSeverity.Error))
							{
								orQuery = Query.Or(orQuery, Query<SyslogEntry>.EQ(x => x.Severity, SyslogSeverity.Error));
							}
							else if (!enumValues.Contains(SyslogSeverity.Err) && enumValues.Contains(SyslogSeverity.Error))
							{
								orQuery = Query.Or(orQuery, Query<SyslogEntry>.EQ(x => x.Severity, SyslogSeverity.Err));
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
				result = result.And(Query.Text(searchText));
				queryParts.Add(searchText);
			}
			QueryString = String.Join(" ", queryParts);
			return ParseDate(maxdate, result);
		}

		private IMongoQuery ParseDate(DateTime? maxdate, IMongoQuery result)
		{
			if (maxdate.HasValue)
			{
				result = result.And(Query<SyslogEntry>.LTE(x => x.Date, new DateTimeOffset(maxdate.Value, DateTimeOffset.Now.Offset)));
			}
			return result;
		}

		private static IMongoQuery OrEnum<T>(Expression<Func<SyslogEntry, T>> property, IEnumerable<string> values, out List<T> enumValues)
			where T : struct
		{
			var v = values.ToArray();
			var matchEnums = v.Where(x => !x.StartsWith("!")).ParseEnum<T>().ToArray();
			var notMatchEnums = v.Where(x => x.StartsWith("!")).Select(x => x.Substring(1)).ParseEnum<T>().ToArray();
			var matchQuery = Query.Null;
			if (matchEnums.Any())
			{
				matchQuery = Query.Or(matchEnums.Select(x => Query<SyslogEntry>.EQ(property, x)));
			}

			var notMatchQuery = Query.Null;
			if (notMatchEnums.Any())
			{
				notMatchQuery = Query.And(notMatchEnums.Select(x => Query.Not(Query<SyslogEntry>.EQ(property, x))));
			}
			enumValues = matchEnums.Concat(notMatchEnums).Distinct().ToList();
			return matchQuery.And(notMatchQuery);
		}

	}
}