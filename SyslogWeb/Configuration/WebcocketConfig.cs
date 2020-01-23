using System;
using System.Collections.Concurrent;
using System.Linq;
using Fleck;
using Microsoft.AspNetCore.Builder;
using SyslogWeb.Websocket;

namespace SyslogWeb
{
    public static class WebcocketConfig
    {
        private static WebSocketServer _server;
        private static readonly ConcurrentDictionary<string, WebsocketClient> Clients = new ConcurrentDictionary<string, WebsocketClient>();
        public static IApplicationBuilder UseWebsocket(this IApplicationBuilder app, MongoDBConfig mongoDb, ushort port)
        {
            app.UseWebSockets();
            _server = new WebSocketServer(String.Format("ws://[::0]:{0}", port));
            try
            {
                _server.Start(x =>
                    {
                        var c = new WebsocketClient(x, mongoDb);
                        x.OnOpen = () => OnOpen(c);
                        x.OnClose = () => OnClose(c);
                        x.OnError = c.OnError;
                        x.OnMessage = c.OnMessage;
                    });
            }
            catch (Exception)
            {
                
            }
            return app;
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