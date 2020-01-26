using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using SyslogWeb.Extensions;
using SyslogWeb.Models;

namespace SyslogWeb.Hubs
{
    public class LogHub:Hub
    {
        private readonly MongoDBConfig _mongoDb;

        public LogHub(MongoDBConfig mongoDb)
        {
            _mongoDb = mongoDb;
        }

        public override Task OnConnectedAsync()
        {
            Paused = new SemaphoreSlim(1, 1);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            Paused.Dispose();
            return base.OnDisconnectedAsync(exception);
        }

        public Task Pause()
        {
            return Paused.WaitAsync(Context.ConnectionAborted);
        }

        public void Resume()
        {
            Paused.Release();
        }

        public async IAsyncEnumerable<object> Tail(WsInfo message, [EnumeratorCancellation]CancellationToken cancellationToken)
        {
            var parser = new QueryParser();
            var query = parser.Parse(message.Search);
            var coll = _mongoDb.SyslogCollection;
            query = query.And(Builders<SyslogEntry>.Filter.Gte(x => x.Id, message.ObjectId));
            
            Regex positiveRegex = null;
            Regex negativeRegex = null;
            if (parser.TextTerms != null)
            {
                var negative = parser.TextTerms.Where(x => x.StartsWith("-")).ToArray();
                var positive = parser.TextTerms.Except(negative).ToArray();
                if (positive.Length > 0)
                {
                    positiveRegex = new Regex(String.Join("|", positive), RegexOptions.IgnoreCase);
                }
                if (negative.Length > 0)
                    negativeRegex = new Regex(String.Join("|", negative.Select(x => x.Substring(1))), RegexOptions.IgnoreCase);
            }
            var cursor = coll.Find(query);
            cursor.Options.NoCursorTimeout = true;
            cursor.Options.CursorType = CursorType.TailableAwait;

            using (var tailable = cursor.ToCursor())
            {
                while (await tailable.MoveNextAsync(cancellationToken))
                {
                    foreach (var x in tailable.Current)
                    {
                        if (x is null) continue;
                        if (x.Id <= message.ObjectId) continue;
                        if (positiveRegex != null && !positiveRegex.IsMatch(x.ToString())) continue;
                        if (negativeRegex != null && negativeRegex.IsMatch(x.ToString())) continue;
                        await Paused.WaitAsync(cancellationToken);
                        Paused.Release();
                        yield return new
                        {
                            x.CssClass,
                            Date = x.Date.LocalDateTime.ToString(),
                            Facility = x.Facility.ToString(),
                            x.Host,
                            x.Program,
                            Severity = x.Severity.ToString(),
                            x.Text,
                            HostAsLink = !parser.Hosts.Contains(x.Host),
                            ProgramAsLink = !parser.Programs.Contains(x.Program)
                        };
                    }
                }
            }
        }

        private SemaphoreSlim Paused
        {
            get => (SemaphoreSlim) Context.Items[nameof(Paused)];
            set => Context.Items[nameof(Paused)] = value;
        }
    }
}
