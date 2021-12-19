using Downloader.Core.Core;
using MMBotGA.backtest;

namespace MMBotGA.downloader;

internal static class DefaultDownloaderExtensions
{
    public static string GetFile(this DefaultDownloader downloader, DownloadTask downloadTask)
    {
        downloader.Download(downloadTask);
        return downloadTask.ToFileName();
    }

    public static BacktestData GetBacktestData(this DefaultDownloader downloader, DownloadTask downloadTask,
        bool reverse, double balance)
    {
        return new BacktestData
        {
            Broker = downloadTask.Exchange.ToLower(),
            Pair = downloadTask.Symbol, // Get broker pair info: /admin/api/brokers/kucoin/pairs
            SourceFile = downloader.GetFile(downloadTask),
            Reverse = reverse,
            Balance = balance
        };
    }
}