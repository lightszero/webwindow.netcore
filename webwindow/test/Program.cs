using System;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            lib.webwindow.WebWindow_Remote windowremote = new lib.webwindow.WebWindow_Remote();
            var b = windowremote.checkElectron(out string version,out string errinfo);
            Console.WriteLine("got electron=" + b);
            if (b)
            {
                Console.WriteLine("electron version=" + version);
                windowremote.startElectron();
            }
            else
            {
                Console.WriteLine("make sure to install electron & nodejs");
                Console.WriteLine("try:: npm install -g electron");

            }
            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
