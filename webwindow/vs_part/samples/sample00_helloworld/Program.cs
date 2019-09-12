using System;
using WebWindow;

namespace sample00_helloworld
{
    class Program
    {
        static bool bexit=false;
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
            op.title = "hello world";
            var window = await WindowRemote.Create(windowmgr, op);

            //eval
            var time = DateTime.Now.ToString();
            await window.Remote_Eval("document.body.innerHTML='hello world<hr/>" + time + "'");
        }
    }
}
