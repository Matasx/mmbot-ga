using Downloader.Core.Core;
using MMBotGA.backtest;
using MMBotGA.data;

namespace MMBotGA.downloader
{
    internal static class DefaultDownloaderExtensions
    {
        public static string GetFile(this DefaultDownloader downloader, DownloadTask downloadTask)
        {
            downloader.Download(downloadTask);
            return downloadTask.ToFileName();
        }

        public static BacktestData GetBacktestData(this DefaultDownloader downloader, Allocation allocation, string dataFolder, DateTimeRange range,
            bool reverse)
        {
            var task = new DownloadTask(dataFolder, allocation.Exchange, allocation.Symbol, range);

            return new BacktestData
            {
                Broker = task.Exchange.ToLower(),
                Pair = allocation.RobotSymbol, // Get broker pair info: /admin/api/brokers/kucoin/pairs
                SourceFile = downloader.GetFile(task),
                Reverse = reverse,
                Balance = allocation.Balance
            };
        }
    }
}