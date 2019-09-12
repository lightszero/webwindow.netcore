using System;
using WebWindow;

namespace sample02_workwithfiles
{
    class Program
    {
        static bool bexit = false;
        static void Main(string[] args)
        {
            MainAsync();
            while (!bexit)
            {
                System.Threading.Thread.Sleep(100);
            }
        }

        static async void MainAsync()
        {
            //init
            WindowMgr windowmgr = new WindowMgr();
            await windowmgr.Init();
            //当GUI全关闭时，让这个进程也退出
            windowmgr.onAllWindowClose += () => bexit = true;

            //create window
            WindowCreateOption op = new WindowCreateOption();
            op.title = "workwithfiles";
            var window = await WindowRemote.Create(windowmgr, op);

            var file = System.IO.Path.GetFullPath("files/tmx.png");
            file = file.Replace("\\", "/");
            //eval sethtmlbody
            //file = "";
            var html = @"<span>image here</span><image src = '" + file + "'></image>";
            var evalstr = "document.body.innerHTML=\"" + html + "\";";
            var v= await window.Remote_Eval(evalstr);

        }
    }
}
