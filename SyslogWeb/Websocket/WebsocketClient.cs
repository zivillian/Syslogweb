using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Fleck;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using SyslogWeb.Extensions;
using SyslogWeb.Models;

namespace SyslogWeb.Websocket
{
	public class WebsocketClient
	{
		private readonly IWebSocketConnection _connection;
        private readonly MongoDBConfig _mongoDb;
        private Thread _worker;
		private ObjectId _minId;
		private bool _closed = false;
		private bool _paused = false;
		private readonly ManualResetEvent _resumeEvent = new ManualResetEvent(false);
		private string[] _selectedHosts;
		private string[] _selectedPrograms;
		private Regex _positiveRegex;
		private Regex _negativeRegex;

		public WebsocketClient(IWebSocketConnection connection, MongoDBConfig mongoDb)
		{
			_connection = connection;
            _mongoDb = mongoDb;
            var info = _connection.ConnectionInfo;
			Id = String.Format("{0}:{1}", info.ClientIpAddress, info.ClientPort);
		}

		public void OnError(Exception ex)
		{
		}

		public void OnMessage(string message)
		{
			if (message == "pause")
			{
				_paused = true;
			}
			else if (message == "resume")
			{
				_paused = false;
				_resumeEvent.Set();
			}
			else
			{
				var cursor = OpenCursor(message);
				_worker = new Thread(o => TailCursor(cursor));
				_worker.Start();
			}
		}

		public void OnClose()
		{
			_closed = true;
			if (_worker != null)
				_worker.Abort();
		}

		private IFindFluent<SyslogEntry, SyslogEntry> OpenCursor(string message)
		{
			var ser = new JsonSerializer();
			var info = ser.Deserialize<WsInfo>(new JsonTextReader(new StringReader(message)));
			
			var parser = new QueryParser();
			var query = parser.Parse(info.Search);
			_selectedHosts = parser.Hosts;
			_selectedPrograms = parser.Programs;
			if (parser.TextTerms != null)
			{
				var negative = parser.TextTerms.Where(x => x.StartsWith("-")).ToArray();
				var positive = parser.TextTerms.Except(negative).ToArray();
				if (positive.Length > 0)
					_positiveRegex = new Regex(String.Join("|", positive), RegexOptions.Compiled | RegexOptions.IgnoreCase);
				if (negative.Length > 0)
					_negativeRegex = new Regex(String.Join("|", negative.Select(x => x.Substring(1))), RegexOptions.Compiled | RegexOptions.IgnoreCase);
			}
			var coll = _mongoDb.SyslogCollection;
			query = query.And(Builders<SyslogEntry>.Filter.Gte(x => x.Id, info.Id));
			var cursor = coll.Find(query);
            cursor.Options.NoCursorTimeout = true;
            cursor.Options.CursorType = CursorType.TailableAwait;
			_minId = info.Id;
			return cursor;
		}

		private void TailCursor(IFindFluent<SyslogEntry, SyslogEntry> cursor)
		{
			try
			{
				var ser = new JsonSerializer();
                foreach (var x in cursor.ToEnumerable())
                {
                    if (_closed) return;
                    if (_paused)
                    {
                        _resumeEvent.WaitOne();
                    }
                    if (x == null) continue;

                    if (x.Id <= _minId) continue;
                    if (_positiveRegex != null && !_positiveRegex.IsMatch(x.ToString())) continue;
                    if (_negativeRegex != null && _negativeRegex.IsMatch(x.ToString())) continue;

                    var writer = new StringWriter();
                    ser.Serialize(writer, new
                    {
                        x.CssClass,
                        Date = x.Date.LocalDateTime.ToString(),
                        Facility = x.Facility.ToString(),
                        x.Host,
                        x.Program,
                        Severity = x.Severity.ToString(),
                        x.Text,
                        HostAsLink = !_selectedHosts.Contains(x.Host),
                        ProgramAsLink = !_selectedPrograms.Contains(x.Program)
                    });
                    Send(writer.ToString());
                }
            }
			catch (IOException ex)
			{
				if (!(ex.InnerException is ThreadAbortException))
				{
					throw;
				}
				//Websocket closed
			}
			catch (ThreadAbortException)
			{
				//Websocket closed
			}
		}

		public void Send(string message)
		{
			_connection.Send(message);
		}

		public string Id { get; private set; }
	}

	public class WsInfo
	{
		[JsonProperty("Id")]
		public string JsonId { get; set; }

		[JsonIgnore]
		public ObjectId Id
		{
			get { return ObjectId.Parse(JsonId); }
		}

		public string Search { get; set; }
	}
}