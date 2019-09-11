using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static light.http.server.httpserver;

namespace WebWindow
{
    public class WindowMgr
    {
        //host的js代码，通常指向html/host/index.js
        public string urlHost
        {
            get;
            private set;
        }
        //win的js代码，通常指向html/win/index.html
        public string urlWin
        {
            get;
            private set;
        }

        /// <summary>
        /// auto close
        /// </summary>
        public bool allWindowClose
        {
            get;
            private set;
        }
        public event Action onAllWindowClose;
        public WindowMgr(string urlHost, string urlWin)
        {
            this.urlHost = urlHost;
            this.urlWin = urlWin;
            this.allWindowClose = false;
        }
        static string cmdfile
        {
            get
            {
                bool isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
                return isWindows ? "cmd" : "bash";
            }
        }
        static string electronfile
        {
            get
            {
                bool isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
                return isWindows ? "electron.cmd" : "./electron";

            }
        }


        public bool hadInit
        {
            get
            {
                return processElectron != null;
            }
        }
        void runCommand(string cmd, out string output, out string errinfo)
        {
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
            info.FileName = cmdfile;
            info.Arguments = "";
            info.RedirectStandardOutput = true;
            info.RedirectStandardInput = true;
            info.RedirectStandardError = true;
            info.CreateNoWindow = true;
            System.Diagnostics.Process p = System.Diagnostics.Process.Start(info);

            p.StandardInput.WriteLine(cmd);
            p.StandardInput.WriteLine("exit");
            p.WaitForExit();

            output = p.StandardOutput.ReadToEnd();
            errinfo = p.StandardError.ReadToEnd();
        }
        /// <summary>
        /// 试着拉起一下electron，成功会得到版本号，没有拉起，你先安装electron
        /// npm i -g electron，国内如果不顺畅，建议用cnpm拉
        /// </summary>
        /// <param name="version"></param>
        /// <param name="errinfo"></param>
        /// <returns></returns>
        public bool checkElectron(out string version, out string errinfo)
        {
            string startcmd = electronfile + " -v";
            runCommand(startcmd, out string got, out errinfo);

            version = null;
            var lines = got.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.Contains(startcmd))
                {
                    var path = line.Substring(0, line.Length - startcmd.Length);

                    for (int nextI = i + 1; nextI < lines.Length; nextI++)
                    {
                        var nextline = lines[nextI];
                        if (nextline.IndexOf(path) == 0) continue;
                        version = nextline;
                        return true;

                    }
                    break;
                }
            }
            return false;
        }
        System.Diagnostics.Process processElectron;
        void runElectron(string args)
        {
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
            info.FileName = cmdfile;
            info.Arguments = "";
            info.RedirectStandardOutput = true;
            info.RedirectStandardInput = true;
            info.RedirectStandardError = true;
            info.CreateNoWindow = true;
            processElectron = System.Diagnostics.Process.Start(info);

            processElectron.StandardInput.WriteLine(electronfile + " " + args);
            processElectron.StandardInput.WriteLine("exit");
            processElectron.OutputDataReceived += (s, r) =>
            {
                if (r.Data == null)
                {
                    //electron had closed;
                    Console.WriteLine("electron close");
                    processElectron = null;
                    return;
                }
                if (r.Data.IndexOf("[TAG]") == 0)
                {
                    var tag = r.Data.Substring(5);
                    this.tags.Enqueue(tag);

                    if (tag.IndexOf("all window closed.") == 0)
                    {
                        allWindowClose = true;
                        if (onAllWindowClose != null)
                            onAllWindowClose();
                    }
                }
                Console.WriteLine("electron=>" + r.Data);
            };
            processElectron.ErrorDataReceived += (s, r) =>
             {
                 if (r.Data == null)
                 {
                     return;
                 }
                 Console.WriteLine("electron error=>" + r.Data);
             };
            //这个从来不触发
            //processElectron.Exited += (s, e) =>
            // {
            //     Console.WriteLine("electron close");
            // };
            processElectron.BeginErrorReadLine();
            processElectron.BeginOutputReadLine();

        }
        Queue<string> tags = new Queue<string>();
        public string TryGetTag()
        {
            if (tags.Count == 0)
                return null;
            return tags.Dequeue();
        }
        int httpport_electron = 0;
        /// <summary>
        /// 初始化elctron，host js 会创建一个服务器，并把端口号返回到输出流，我们收集这个端口号
        /// 窗口操作都是通过这个端口rpc
        /// </summary>
        /// <param name="hosturl"></param>
        /// <returns></returns>
        ///            

        light.http.server.httpserver server_window;
        int httpport_window;
        private IWebSocketPeer onwebsocket(System.Net.WebSockets.WebSocket websocket)
        {
            return new WebSocketSession(websocket);
        }
        public async Task<bool> Init()
        {
            if (this.server_window == null)
            {
                this.httpport_window = -1;
                this.server_window = new light.http.server.httpserver();
                for (var port = 8888; port < 60000; port++)
                {
                    try
                    {
                        httpport_window = port;
                        this.server_window.Start(port);
                        this.server_window.SetWebsocketAction("/ws", onwebsocket);
                        Console.WriteLine("httpport_window=" + this.httpport_window);
                        break;
                    }
                    catch
                    { }
                }
                if (this.httpport_window == -1)
                    throw new Exception("can not init server for window");

            }
            var b = checkElectron(out string version, out string errinfo);
            if (b)
            {
                Console.WriteLine("electron version=" + version);
            }
            else
            {
                Console.WriteLine("make sure to install electron & nodejs");
                Console.WriteLine("try:: npm install -g electron");
                return false;
            }
            //start server and gen url
            //string url = "http://127.0.0.1:88445/__windowmgr.js&checkcode=xxx";

            //electron 可以通过commandline 传递信息
            string url = "ws://127.0.0.1:" + httpport_window + "/ws";
            //start electron
            string args = this.urlHost + " --controlurl=" + url;
            runElectron(args);

            while (true)
            {
                await Task.Delay(1);
                var tag = TryGetTag();
                if (tag!=null)
                {
                    if (tag.IndexOf("listen at:") == 0)
                    {
                        var portstr = tag.Substring("listen at:".Length);
                        httpport_electron = int.Parse(portstr);
                        return true;
                    }
                    if (tag.IndexOf("listen fail") == 0)
                    {
                        return false;
                    }

                }
            }
        }

        /// <summary>
        /// rpc指令，退出electron
        /// </summary>
        /// <returns></returns>
        public async Task app_exit()
        {
            var wc = new System.Net.WebClient();
            var str = await wc.DownloadStringTaskAsync("http://localhost:" + this.httpport_electron + "/?method=quit");
            Console.WriteLine(str);
        }
        /// <summary>
        /// rpc指令，创建一个窗口，从指定的url，url可以是本地文件，也可以是http服务器
        /// </summary>
        /// <param name="option"></param>
        /// <param name="_url"></param>
        /// <returns></returns>
        public async Task<int> window_create(WindowCreateOption option, string _url, int parentwindow = -1)
        {
            Newtonsoft.Json.Linq.JArray _params = new JArray();
            JObject objwin = option.ToJson();
            _params.Add(objwin);
            _params.Add(_url);
            var url = "http://localhost:" + this.httpport_electron + "/?method=window.create&params=" + _params.ToString().Replace("\r\n", "");
            var wc = new System.Net.WebClient();
            var str = await wc.DownloadStringTaskAsync(url);
            var jsonrecv = JObject.Parse(str);
            var id = (int)jsonrecv["winid"];
            return id;
        }
        /// <summary>
        /// rpc指令，关闭一个窗口
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task window_close(int id)
        {
            Newtonsoft.Json.Linq.JArray _params = new JArray();
            _params.Add(id);
            var url = "http://localhost:" + this.httpport_electron + "/?method=window.close&params=" + _params.ToString().Replace("\r\n", "");
            var wc = new System.Net.WebClient();
            var str = await wc.DownloadStringTaskAsync(url);
        }
        ///待加的rpc指令
        public async Task window_show(int id)
        {
            Newtonsoft.Json.Linq.JArray _params = new JArray();
            _params.Add(id);
            var url = "http://localhost:" + this.httpport_electron + "/?method=window.show&params=" + _params.ToString().Replace("\r\n", "");
            var wc = new System.Net.WebClient();
            var str = await wc.DownloadStringTaskAsync(url);
        }
        public async Task window_hide(int id)
        {
            Newtonsoft.Json.Linq.JArray _params = new JArray();
            _params.Add(id);
            var url = "http://localhost:" + this.httpport_electron + "/?method=window.hide&params=" + _params.ToString().Replace("\r\n", "");
            var wc = new System.Net.WebClient();
            var str = await wc.DownloadStringTaskAsync(url);
        }
    }
}
