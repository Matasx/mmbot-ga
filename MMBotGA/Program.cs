using System.Net;
using System.Threading.Tasks;
using Terminal.Gui;

namespace MMBotGA
{
    internal class Program
    {
        private static void Main()
        {
            ServicePointManager.DefaultConnectionLimit = 10;

            Application.Init();

            var window = new MainWindow(Application.Top);

            Task.WaitAll(
                Task.Run(() => Application.Run()),
                Task.Run(window.Run)
            );
        }
    }
}
