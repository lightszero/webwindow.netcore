using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace lib.webwindow
{
    public class WebWindow_Remote
    {
        static string electronCMD
        {
            get
            {
                bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                return (isWindows ? "electron.cmd" : "./electron");
            }
        }
        void runCMD(string cmd, out string output, out string errinfo)
        {
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            info.FileName = isWindows ? "cmd" : "bash";
            info.UseShellExecute = false;
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
        System.Diagnostics.Process currentProcess;
        void runApp(string cmd)
        {
            if (currentProcess != null) throw new Exception("only run one app once.");
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            info.FileName = isWindows ? "cmd" : "bash";
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;
            info.RedirectStandardInput = true;
            info.RedirectStandardError = true;
            info.CreateNoWindow = true;
            currentProcess = System.Diagnostics.Process.Start(info);
            currentProcess.StandardInput.WriteLine(cmd);
            currentProcess.StandardInput.WriteLine("exit");
            currentProcess.Exited += _app_exit;
            currentProcess.OutputDataReceived += _app_output;
            currentProcess.ErrorDataReceived += _app_error;
            currentProcess.BeginErrorReadLine();
            currentProcess.BeginOutputReadLine();
        }
        void _app_exit(object sender, EventArgs arg)
        {
            Console.WriteLine("quit");
            currentProcess = null;

        }
        void _app_error(object sender, DataReceivedEventArgs arg)
        {
            Console.WriteLine("error:" + arg.Data);
        }
        void _app_output(object sender, DataReceivedEventArgs arg)
        {
            Console.WriteLine("output:" + arg.Data);
        }
        public bool checkElectron(out string version, out string errinfo)
        {
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            string cmd = electronCMD + " -v";

            runCMD(cmd, out string got, out errinfo);

            version = null;
            var lines = got.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.Contains(cmd))
                {
                    var path = line.Substring(0, line.Length - cmd.Length);

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
        public void startElectron()
        {
            string url = "http://www.163.com&checkcode=xxx";
            string cmd = electronCMD + " " + url;

            runApp(cmd);
            //}

            //System.Threading.Tasks.Task.WaitAll(ElectronNET.API.Electron.WindowManager.CreateWindowAsync("http://www.163.com"));
            //start server and gen url
            //string url = "http://127.0.0.1:88445/__windowmgr.js&checkcode=xxx";
            //         string url = "http://www.163.com&checkcode=xxx";
            //         //start electron
            //         string startcmd = "electron " + url;
            //         startCMD(startcmd, out string got, out string errinfo);
            //if(string.IsNullOrEmpty(errinfo)==false)
            //         {
            //             Console.WriteLine("fail:" + errinfo);
            //         }
        }
        public void start()
        {

        }
    }
}
