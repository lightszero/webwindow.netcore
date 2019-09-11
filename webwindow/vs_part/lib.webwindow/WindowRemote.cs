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
        WindowMgr WindowMgr;
        int winid;
        protected WindowRemote(WindowMgr mgr,int windowid)
        {
            this.WindowMgr = mgr;
            this.winid = windowid;
        }
        public static async Task<WindowRemote> Create( WebWindow.WindowMgr mgr, WindowCreateOption op)
        {
            if (mgr.hadInit)
                throw new Exception("windowmgr has not inited.");

            var url = mgr.urlWin;
            var windowid = await mgr.window_create(op, url);

            var win = new WindowRemote(mgr, windowid);
            return win;
        }

    }
}
