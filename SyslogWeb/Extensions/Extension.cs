using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Driver;
using SyslogWeb.Models;

namespace SyslogWeb.Extensions
{
    public static class Extension
    {
        public static bool IsDebug(this IHtmlHelper htmlHelper)
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }

        public static FilterDefinition<T> And<T>(this FilterDefinition<T> left, FilterDefinition<T> right)
        {
            if (left == null || left == FilterDefinition<T>.Empty) return right;
            if (right == null || right == FilterDefinition<T>.Empty) return left;
            return Builders<T>.Filter.And(left, right);
        }

        public static FilterDefinition<T> Or<T>(this FilterDefinition<T> left, FilterDefinition<T> right)
        {
            if (left == null || left == FilterDefinition<T>.Empty) return right;
            if (right == null || right == FilterDefinition<T>.Empty) return left;
            return Builders<T>.Filter.Or(left, right);
        }

        public static IEnumerable<T> ParseEnum<T>(this IEnumerable<string> values)
            where T : struct
        {
            foreach (var value in values)
            {
                if (Enum.TryParse(value, true, out T parsed))
                {
                    yield return parsed;
                }
            }
        }

        public static FilterDefinition<SyslogEntry> Or(this IEnumerable<string> values,
            Expression<Func<SyslogEntry, string>> property)
        {
            var v = values.ToArray();
            var matchQueries = v.Where(x => !x.StartsWith("!")).Select(x => Builders<SyslogEntry>.Filter.Eq(property, x)).ToArray();
            var notMatchQueries = v.Where(x => x.StartsWith("!")).Select(x => Builders<SyslogEntry>.Filter.Not(Builders<SyslogEntry>.Filter.Eq(property, x.Substring(1)))).ToArray();
            var matchQuery = FilterDefinition<SyslogEntry>.Empty;
            if (matchQueries.Any())
            {
                matchQuery = Builders<SyslogEntry>.Filter.Or(matchQueries);
            }

            var notMatchQuery = FilterDefinition<SyslogEntry>.Empty;
            if (notMatchQueries.Any())
            {
                notMatchQuery = Builders<SyslogEntry>.Filter.And(notMatchQueries);
            }
            return matchQuery.And(notMatchQuery);
        }
    }
}