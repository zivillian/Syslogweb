using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Web;
using System.Web.WebSockets;
using Fleck;
using SyslogWeb.Websocket;

namespace SyslogWeb
{
	public class WebcocketConfig
	{
		private static WebSocketServer _server;
		private static readonly ConcurrentDictionary<string, WebsocketClient> Clients = new ConcurrentDictionary<string, WebsocketClient>();
		public static void Register()
		{
			_server = new WebSocketServer(String.Format("ws://[::0]:{0}", Properties.Settings.Default.WebsocketPort));
			try
			{
				_server.Start(x =>
					{
						var c = new WebsocketClient(x);
						x.OnOpen = () => OnOpen(c);
						x.OnClose = () => OnClose(c);
						x.OnError = c.OnError;
						x.OnMessage = c.OnMessage;
					});
			}
			catch (Exception)
			{
				
			}
		}

		public static void Broadcast(WebsocketClient sender, string message)
		{
			foreach (var client in Clients.Values.Where(x => x != sender))
			{
				client.Send(message);
			}
		}

		private static void OnClose(WebsocketClient connection)
		{
			WebsocketClient old;
			Clients.TryRemove(connection.Id, out old);
			connection.OnClose();
		}

		private static void OnOpen(WebsocketClient connection)
		{
			Clients.AddOrUpdate(connection.Id, connection, (k,v)=>connection);
		}
	}
}