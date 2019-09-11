using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static light.http.server.httpserver;

namespace WebWindow
{
    public class WebSocketSession : IWebSocketPeer
    {
        System.Net.WebSockets.WebSocket socket;
        WindowRemote bindwin;
        int winid;
        public WebSocketSession(System.Net.WebSockets.WebSocket socket)
        {
            this.socket = socket;
            this.winid = -1;
        }
        public async Task Send(string cmd, JArray vars)
        {
            JObject send = new JObject();
            send["cmd"] = cmd;
            send["vars"] = vars;
            var sendtxt = send.ToString();
            var bs = System.Text.Encoding.UTF8.GetBytes(sendtxt);
            await this.socket.SendAsync(bs, System.Net.WebSockets.WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);
        }
        public async Task OnConnect()
        {
            Console.WriteLine("WebSocketSession:OnConnect" + socket.GetHashCode());
        }

        public async Task OnDisConnect()
        {
            if (bindwin != null)
                bindwin.SessionClose();
        }

        public async Task OnRecv(MemoryStream stream, int count)
        {
            if (count == 0)
            {
                await OnDisConnect();
                return;
            }
            //路由消息
            var buf = new byte[count];
            stream.Read(buf, 0, count);
            var txt = System.Text.Encoding.UTF8.GetString(buf);

            var json = JObject.Parse(txt);
            var cmd = json["cmd"].ToString();
            var vars = (JArray)json["vars"];

            if (cmd == "init")
            {
                this.winid = (int)vars[0];
                bindwin = WindowRemote.Get(this.winid);
                bindwin.BindSession(this);
            }

            if (bindwin == null)
                Console.WriteLine("error socket msg:" + txt);
            else
                bindwin.OnRecv(cmd, vars);

        }
    }
}
