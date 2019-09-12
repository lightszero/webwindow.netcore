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
            op.title = "workwithhtml";
            var window = await WindowRemote.Create(windowmgr, op);

            var file = System.IO.Path.GetFullPath("html/mypage.html");
            file = file.Replace("\\", "/");
            //eval sethtmlbody
            //file = "";
            var html = @"<iframe src = '" + file + "'  style='width: 100%; height: 100%; border: 0px'></iframe>";
            var evalstr = "document.body.innerHTML=\"" + html + "\";";
            var v = await window.Remote_Eval(evalstr);

            ////eval bindevent
            //var bindjs = @" var btn = document.getElementById('btn');
            //                btn.onclick = function () {
            //                    var txt = document.getElementById('textbox');
            //                    __api.sendback(['click', txt.value]);
            //                };";
            ////use __api.sendback([]) to send a json array to dotnet core.
            //await window.Remote_Eval(bindjs);


            //watch event
            window.OnSendBack += (args) =>
            {
                Console.WriteLine("recv:" + args.ToString());
            };
        }
    }
}
