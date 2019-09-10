using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace lib.webwindow
{
    public class WebWindow_Remote
    {
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
                if (r.Data == null) return;
                if(r.Data.IndexOf("[TAG]")==0)
                {
                    this.tags.Enqueue(r.Data.Substring(5));
                }
                Console.WriteLine("electron=>" + r.Data);
            };
            processElectron.ErrorDataReceived += (s, r) =>
             {
                 Console.WriteLine("electron error=>" + r.Data);
             };
            processElectron.Exited += (s, e) =>
             {
                 Console.WriteLine("electron close");
             };
            processElectron.BeginErrorReadLine();
            processElectron.BeginOutputReadLine();

        }
        Queue<string> tags = new Queue<string>();
        int httpport = 0;
        public async Task<bool> Init()
        {
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
            string url = "ws://127.0.0.1:78474";
            //start electron
            string args = "html/index.js --controlurl=" + url;
            runElectron(args);

            while (true)
            {
                await Task.Delay(1);
                if(tags.Count>0)
                {
                    var tag = tags.Dequeue();
                    if(tag.IndexOf("listen at:")==0)
                    {
                        var portstr = tag.Substring("listen at:".Length);
                        httpport = int.Parse(portstr);
                        return true;
                    }
                    if(tag.IndexOf("listen fail") ==0)
                    {
                        return false;
                    }
                }
            }
        }
        public async Task app_exit()
        {
            var wc = new System.Net.WebClient();
            var str=await wc.DownloadStringTaskAsync("http://localhost:" + this.httpport + "/?method=quit");
            Console.WriteLine(str);
        }
        public async Task<int> window_create(string title)
        {
            Newtonsoft.Json.Linq.JArray _params = new JArray();
            JObject objwin = new JObject();
            objwin["title"] = title;
            _params.Add( objwin);
            _params.Add("http://www.baidu.com");
            var url = "http://localhost:" + this.httpport + "/?method=window.create&params=" + _params.ToString().Replace("\r\n", "");
            var wc = new System.Net.WebClient();
            var str =await wc.DownloadStringTaskAsync(url);
            var jsonrecv = JObject.Parse(str);
            var id =(int)jsonrecv["winid"];
            return id;
        }
        public async Task window_close(int id)
        {
            Newtonsoft.Json.Linq.JArray _params = new JArray();
            _params.Add(id);
            var url = "http://localhost:" + this.httpport + "/?method=window.close&params=" + _params.ToString().Replace("\r\n", "");
            var wc = new System.Net.WebClient();
            var str = await wc.DownloadStringTaskAsync(url);
        }
    }
}
