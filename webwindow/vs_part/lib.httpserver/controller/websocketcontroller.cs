using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace light.http.server
{
    public class WebSocketController : IController
    {
        httpserver.deleWebSocketCreator CreatePeer;
        //httpserver.onProcessWebsocket onEvent;
        public WebSocketController(httpserver.deleWebSocketCreator onCreator)
        {
            this.CreatePeer = onCreator;
        }
        public async Task ProcessAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket websocket = null;
                httpserver.IWebSocketPeer peer = null;
                try
                {
                    websocket = await context.WebSockets.AcceptWebSocketAsync();
                    peer = CreatePeer(websocket);
                    await peer.OnConnect();
                }
                catch
                {
                    Console.CursorLeft = 0;
                    Console.WriteLine("error on connect.");
                }
                try
                {
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream(1024 * 1024))
                    {
                        byte[] buf = new byte[1024];
                        ArraySegment<byte> buffer = new ArraySegment<byte>(buf);
                        while (websocket.State == System.Net.WebSockets.WebSocketState.Open)
                        {
                            
                            var recv = await websocket.ReceiveAsync(buffer, System.Threading.CancellationToken.None);
                            ms.Write(buffer.Array, buffer.Offset, recv.Count);
                            if (recv.EndOfMessage)
                            {
                                var count = ms.Position;

                                ms.Position = 0;
                                await peer.OnRecv(ms,(int)count);// .onEvent(httpserver.WebsocketEventType.Recieve, websocket, bytes);


                                ms.Position = 0;
                            }
                        }
                    }
                }
                catch (Exception err)
                {
                    Console.CursorLeft = 0;
                    Console.WriteLine("error on recv.");
                }
                try
                {
                    //await context.Response.WriteAsync("");
                    await peer.OnDisConnect();// onEvent(httpserver.WebsocketEventType.Disconnect, websocket);
                }
                catch (Exception err)
                {
                    Console.CursorLeft = 0;
                    Console.WriteLine("error on disconnect.");
                }

            }
            else
            {

            }

        }
    }
}
