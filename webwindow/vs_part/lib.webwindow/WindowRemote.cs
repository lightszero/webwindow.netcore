using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebWindow
{

    /// <summary>
    /// 远程管理的window
    /// </summary>
    public class WindowRemote
    {
        static Dictionary<int, WindowRemote> allwindow;
        public static WindowRemote Get(int id)
        {
            if (allwindow.ContainsKey(id))
                return allwindow[id];
            return null;
        }

        static void RecordWin(WindowRemote win)
        {
            if (allwindow == null)
                allwindow = new Dictionary<int, WindowRemote>();
            allwindow[win.winid] = win;
        }

        public void OnRecv(string cmd, JArray vars)
        {
            Console.WriteLine("Onrecv:" + cmd);
            if(cmd=="settitle_back")
            {
                lock (this)
                {
                    tag_title = true;
                }
            }
            if(cmd=="eval_back")
            {
                lock (this)
                {
                    tag_eval = vars[0];
                }
            }
        }
        bool tag_title;
        public async Task Remote_SetTitle(string title)
        {
            lock (this)
            {
                tag_title = false;
            }
            await bindSession.Send("settitle", new JArray(title));
            while(true)
            {
                lock (this)
                {
                    if(tag_title)
                    {
                        return;
                    }
                }
                await Task.Delay(1);
            }
        }

        JToken tag_eval;
        public async Task<JToken> Remote_Eval(string jscode)
        {
            lock (this)
            {
                tag_eval = null;
            }
            await bindSession.Send("eval", new JArray(jscode));
            while (true)
            {
                lock (this)
                {
                    if (tag_eval!=null)
                    {
                        return tag_eval.DeepClone();
                    }
                }
                await Task.Delay(1);
            }
        }
        public void BindSession(WebSocketSession session)
        {
            Console.WriteLine("Bind to Session.");

            this.bindSession = session;
        }
        public void SessionClose()
        {
            Console.WriteLine("Session close.");

            allwindow.Remove(this.winid);
            this.bindSession = null;
        }




        WindowMgr WindowMgr;
        WebSocketSession bindSession;
        public int winid
        {
            get;
            private set;
        }
        protected WindowRemote(WindowMgr mgr,int windowid)
        {
            this.WindowMgr = mgr;
            this.winid = windowid;
        }
        public bool hasInit
        {
            get
            {
                return bindSession != null;
            }
        }
        public static async Task<WindowRemote> Create( WebWindow.WindowMgr mgr, WindowCreateOption op)
        {
            if (!mgr.hadInit)
                throw new Exception("windowmgr has not inited.");

            var url = mgr.urlWin;
            var windowid = await mgr.window_create(op, url);

            var win = new WindowRemote(mgr, windowid);

            RecordWin(win);
            while(!win.hasInit)
            {
                await Task.Delay(100);
            }
            return win;
        }

    }
}
