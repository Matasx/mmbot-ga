using System.Net;
using System.Threading.Tasks;
using log4net;
using Terminal.Gui;

namespace MMBotGA
{
    internal class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        private static void Main()
        {
            ServicePointManager.DefaultConnectionLimit = 10;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Log.Info("Starting");

            Application.Init();

            var window = new MainWindow(Application.Top);

            Task.WaitAll(
                Task.Run(() => Application.Run()),
                Task.Run(window.Run)
            );
        }
    }
}
