using System;

namespace test
{
    class Program
    {
        static async void MainAsync()
        {
            Console.WriteLine("Hello World!");
            lib.webwindow.WebWindow_Remote windowremote = new lib.webwindow.WebWindow_Remote();

            //初始化
            var b = await windowremote.Init();
            if (!b)
            {
                bexit = true;
            }
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
                        await windowremote.app_exit();
                        bexit = true;

                        return;
                    }
                    if (line == "openwin")
                    {
                       var wid= await windowremote.window_create("李白");
                        Console.WriteLine("openwin=" + wid);
                    }
                    if (line.IndexOf("closewin") == 0)
                    {
                        var words = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                        var wid = int.Parse(words[1]);
                        await windowremote.window_close(wid);
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
