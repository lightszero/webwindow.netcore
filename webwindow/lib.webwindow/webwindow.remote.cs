using System;
using System.Collections.Generic;
using System.Text;

namespace lib.webwindow
{
    public class WebWindow_Remote
    {
		 void startCMD(string startcmd, out string output,out string errinfo)
        {
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
            info.FileName = "cmd";
            info.Arguments = "";
            info.RedirectStandardOutput = true;
            info.RedirectStandardInput = true;
            info.RedirectStandardError = true;
            info.CreateNoWindow = true;
            System.Diagnostics.Process p = System.Diagnostics.Process.Start(info);

            p.StandardInput.WriteLine(startcmd);
            p.StandardInput.WriteLine("exit");
            p.WaitForExit();

            output = p.StandardOutput.ReadToEnd();
            errinfo = p.StandardError.ReadToEnd();
        }
        public bool checkElectron(out string version, out string errinfo)
        {
            string startcmd = "electron -v";
            startCMD(startcmd,out string got,out errinfo);

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
        public void OpenWindow()
        {
            //start server and gen url
            //string url = "http://127.0.0.1:88445/__windowmgr.js&checkcode=xxx";
            string url = "http://www.163.com&checkcode=xxx";
            //start electron
            string startcmd = "electron " + url;
            startCMD(startcmd, out string got, out string errinfo);
			if(string.IsNullOrEmpty(errinfo)==false)
            {
                Console.WriteLine("fail:" + errinfo);
            }
        }
    }
}
