using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace light.http.server
{


    public class httpserver
    {
        public httpserver()
        {
            onHttpEvents = new System.Collections.Concurrent.ConcurrentDictionary<string, IController>();
        }
        private IWebHost host;
        public System.Collections.Concurrent.ConcurrentDictionary<string, IController> onHttpEvents;
        deleProcessHttp onHttp404;



        public void Start(int port, int portForHttps = 0, string pfxpath = null, string password = null)
        {
            host = new WebHostBuilder().UseKestrel((options) =>
            {
                options.Listen(IPAddress.Any, port, listenOptions =>
                  {

                  });
                if (portForHttps != 0)
                {
                    options.Listen(IPAddress.Any, portForHttps, listenOptions =>
                      {
                          //if (!string.IsNullOrEmpty(sslCert))
                          //if (useHttps)
                          listenOptions.UseHttps(pfxpath, password);
                          //sslCert, password);
                      });
                }
            }).Configure(app =>
            {

                app.UseWebSockets();
                app.UseResponseCompression();

                app.Run(ProcessAsync);
            }).ConfigureServices(services =>
            {
                services.AddResponseCompression(options =>
                {
                    options.EnableForHttps = false;
                    options.Providers.Add<GzipCompressionProvider>();
                    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/json" });
                });

                services.Configure<GzipCompressionProviderOptions>(options =>
                {
                    options.Level = CompressionLevel.Fastest;
                });
            }).Build();

            host.Start();
        }

        public void AddJsonRPC(string path, string method, JSONRPCController.ActionRPC action)
        {
            if (onHttpEvents.ContainsKey(path) == false)
            {
                onHttpEvents[path] = new JSONRPCController();
            }
            var jsonc = onHttpEvents[path] as JSONRPCController;
            jsonc.AddAction(method, action);
        }
        public void SetJsonRPCFail(string path, JSONRPCController.ActionRPCFail action)
        {
            if (onHttpEvents.ContainsKey(path) == false)
            {
                onHttpEvents[path] = new JSONRPCController();
            }
            var jsonc = onHttpEvents[path] as JSONRPCController;
            jsonc.SetFailAction(action);
        }
        public void SetHttpAction(string path, deleProcessHttp httpaction)
        {
            onHttpEvents[path] = new ActionController(httpaction);
        }
        public void SetWebsocketAction(string path, deleWebSocketCreator websocketaction)
        {
            onHttpEvents[path] = new WebSocketController(websocketaction);
        }
        public void SetHttpController(string path, IController controller)
        {
            onHttpEvents[path] = controller;
        }
        public void SetFailAction(deleProcessHttp httpaction)
        {
            onHttp404 = httpaction;
        }
        public delegate Task deleProcessHttp(HttpContext context);
        public interface IWebSocketPeer
        {
            Task OnConnect();
            Task OnRecv(System.IO.MemoryStream stream, int count);
            Task OnDisConnect();
        }
        public delegate IWebSocketPeer deleWebSocketCreator(System.Net.WebSockets.WebSocket websocket);
        //public enum WebsocketEventType
        //{
        //    Connect,
        //    Disconnect,
        //    Recieve,
        //}
        //public delegate Task onProcessWebsocket(WebsocketEventType type, System.Net.WebSockets.WebSocket context, byte[] message = null);


        private async Task ProcessAsync(HttpContext context)
        {
            try
            {
                var path = context.Request.Path.Value;
                if (onHttpEvents.TryGetValue(path.ToLower(), out IController controller))
                {
                    await controller.ProcessAsync(context);
                }
                else
                {
                    await onHttp404(context);
                }
            }
            catch
            {

            }
        }
    }
}
