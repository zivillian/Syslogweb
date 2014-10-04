using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using Fleck;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using SyslogWeb.Extensions;
using SyslogWeb.Models;

namespace SyslogWeb.Websocket
{
	public class WebsocketClient
	{
		private readonly IWebSocketConnection _connection;
		private Thread _worker;
		private ObjectId _minId;
		private bool _closed = false;
		private bool _paused = false;
		private readonly ManualResetEvent _resumeEvent = new ManualResetEvent(false);
		private string[] _selectedHosts;
		private string[] _selectedPrograms;
		private Regex _positiveRegex;
		private Regex _negativeRegex;

		public WebsocketClient(IWebSocketConnection connection)
		{
			_connection = connection; 
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

		private MongoCursor<SyslogEntry> OpenCursor(string message)
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
			var coll = MongoDBConfig.SyslogCollection;
			query = query.And(Query<SyslogEntry>.GTE(x => x.Id, info.Id));
			var cursor = coll.Find(query);
			cursor.SetFlags(QueryFlags.AwaitData | QueryFlags.NoCursorTimeout | QueryFlags.TailableCursor);
			_minId = info.Id;
			return cursor;
		}

		private void TailCursor(MongoCursor<SyslogEntry> cursor)
		{
			try
			{
				var ser = new JsonSerializer();
				using (var enumerator = new MongoCursorEnumerator<SyslogEntry>(cursor))
				{
					while (!_closed)
					{
						if (_paused)
						{
							_resumeEvent.WaitOne();
						}
						if (enumerator.MoveNext())
						{
							if (enumerator.Current == null) continue;
							var x = enumerator.Current;
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
						else if (enumerator.IsDead)
						{
							break;
						}
					}
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