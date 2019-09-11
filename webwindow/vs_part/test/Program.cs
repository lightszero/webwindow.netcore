using System;
using WebWindow;
namespace test
{
    class Program
    {
        static async void MainAsync()
        {
            Console.WriteLine("Hello World!");

            var hosturl = System.IO.Path.Join(System.IO.Directory.GetCurrentDirectory(), "../../../../../html/host/index.js");
            hosturl = System.IO.Path.GetFullPath(hosturl);
            var winurl = System.IO.Path.Join(System.IO.Directory.GetCurrentDirectory(), "../../../../../html/win/index.html");
            winurl = "file://" + System.IO.Path.GetFullPath(winurl);
            WindowMgr windowmgr = new WindowMgr(hosturl, winurl);


            //让程序和ui 一起退出
            windowmgr.onAllWindowClose += () =>
              {
                  bexit = true;
              };
            //初始化
            //由于目前的设计 windowremote 会突然关闭,所以需要在openwin之前检查
            //var b = await windowremote.Init(hosturl);
            //if (!b)
            //{
            //    bexit = true;
            //}
            while (!bexit)
            {
                Console.WriteLine("====type exit to quit app.");
                Console.WriteLine("====type openwin to create a window.");
                Console.WriteLine("====type closewin [id] to close a window.");
                Console.Write(">");
                var line = Console.ReadLine();

                try
                {
                    if (line == "exit")
                    {
                        if (windowmgr.hadInit)
                        {
                            await windowmgr.app_exit();
                        }
                        bexit = true;

                        return;
                    }
                    if (line == "opennativewin")
                    {
                        if (windowmgr.hadInit == false)
                            await windowmgr.Init();

                        var op = new WindowCreateOption();
                        op.title = "李白";
                        var wid = await windowmgr.window_create(op, "d:\\1.html");
                        Console.WriteLine("openwin=" + wid);
                    }
                    if (line == "openwin")
                    {
                        if (windowmgr.hadInit == false)
                            await windowmgr.Init();
                        var op = new WindowCreateOption();
                        op.title = "李白";

                        WindowRemote window = await WindowRemote.Create(windowmgr, op);
                        await window.Remote_SetTitle("hello that's so cool.");
                        await window.Remote_Eval("document.body.innerHTML='testfix<hr/>adafdf';");
                    }
                    if (line.IndexOf("closewin") == 0)
                    {
                        if (windowmgr.hadInit)
                        {
                            var words = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                            var wid = int.Parse(words[1]);
                            await windowmgr.window_close(wid);
                        }
                    }
                    if (line.IndexOf("showwin") == 0)
                    {
                        if (windowmgr.hadInit)
                        {
                            var words = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                            var wid = int.Parse(words[1]);
                            await windowmgr.window_show(wid);
                        }
                    }
                    if (line.IndexOf("hidewin") == 0)
                    {
                        if (windowmgr.hadInit)
                        {
                            var words = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                            var wid = int.Parse(words[1]);
                            await windowmgr.window_hide(wid);
                        }
                    }
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.Message);
                }
            }
        }
        static bool bexit = false;
        static void Main(string[] args)
        {
            MainAsync();
            while (!bexit)
            {
                System.Threading.Thread.Sleep(100);
            }

        }
    }
}
