using System;
using WebWindow;

namespace sample01_sendback
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
            op.title = "sendback";
            var window = await WindowRemote.Create(windowmgr, op);

            //eval sethtmlbody
            var html = @"   <span>input text here</span>
                            <input id = 'textbox' type = 'text'></input>
                            <button id = 'btn'> click me </button>";
            await window.Remote_Eval("document.body.innerHTML=\"" + html + "\"");

            //eval bindevent
            var bindjs = @" var btn = document.getElementById('btn');
                            btn.onclick = function () {
                                var txt = document.getElementById('textbox');
                                __api.sendback(['click', txt.value]);
                            };";
            //use __api.sendback([]) to send a json array to dotnet core.
            await window.Remote_Eval(bindjs);


            //watch event
            window.OnSendBack += (args) =>
             {
                 Console.WriteLine("recv:" + args.ToString());
             };
        }
    }
}
