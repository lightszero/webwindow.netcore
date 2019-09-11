using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static light.http.server.httpserver;

namespace WebWindow
{
    class WebSocketSession : IWebSocketPeer
    {
        System.Net.WebSockets.WebSocket socket;
        public WebSocketSession(System.Net.WebSockets.WebSocket socket)
        {
            this.socket = socket;
           
        }
        public async Task OnConnect()
        {
            Console.WriteLine("WebSocketSession:OnConnect" + socket.GetHashCode());
            return;
        }

        public async Task OnDisConnect()
        {
            Console.WriteLine("WebSocketSession:OnDisConnect" + socket.GetHashCode());
        }

        public async Task OnRecv(MemoryStream stream, int count)
        {
            Console.WriteLine("WebSocketSession:OnRecv=" + count);
        }
    }
}
