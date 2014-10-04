using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using SyslogWeb.Models;
using System.Web.Mvc;

namespace SyslogWeb.Extensions
{
	public static class Extension
	{
        public static bool IsDebug(this HtmlHelper htmlHelper)
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
		public static IMongoQuery And(this IMongoQuery left, IMongoQuery right)
		{
			if (left == null || left == Query.Null) return right;
			if (right == null || right == Query.Null) return left;
			return Query.And(left, right);
		}

		public static IEnumerable<T> ParseEnum<T>(this IEnumerable<string> values)
			where T : struct
		{
			foreach (var value in values)
			{
				T parsed;
				if (Enum.TryParse(value, true, out parsed))
				{
					yield return parsed;
				}
			}
		}

		public static IMongoQuery Or(this IEnumerable<string> values, Expression<Func<SyslogEntry, string>> property)
		{
			var v = values.ToArray();
			var matchQueries = v.Where(x => !x.StartsWith("!")).Select(x => Query<SyslogEntry>.EQ(property, x)).ToArray();
			var notMatchQueries = v.Where(x => x.StartsWith("!")).Select(x => Query.Not(Query<SyslogEntry>.EQ(property, x.Substring(1)))).ToArray();
			var matchQuery = Query.Null;
			if (matchQueries.Any())
			{
				matchQuery = Query.Or(matchQueries);
			}

			var notMatchQuery = Query.Null;
			if (notMatchQueries.Any())
			{
				notMatchQuery = Query.And(notMatchQueries);
			}
			return matchQuery.And(notMatchQuery);
		}
	}
}