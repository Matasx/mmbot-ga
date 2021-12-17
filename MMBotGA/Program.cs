using System.Net;
using Terminal.Gui;

namespace MMBotGA
{
    internal class Program
    {
        private static void Main()
        {
            ServicePointManager.DefaultConnectionLimit = 10;

            Application.Init();

            new MainWindow(Application.Top).RunTask();

            Application.Run();
        }
    }
}
