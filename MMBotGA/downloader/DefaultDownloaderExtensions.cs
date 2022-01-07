using System;
using Downloader.Core.Core;
using MMBotGA.backtest;
using MMBotGA.data;

namespace MMBotGA.downloader
{
    internal static class DefaultDownloaderExtensions
    {
        public static BacktestData GetBacktestData(this DefaultDownloader downloader, Allocation allocation, string dataFolder, DateTimeRange range,
            bool reverse, DateTime? start = null, int? limit = null, int? offset = null)
        {
            var task = new DownloadTask(dataFolder, allocation.Exchange, allocation.Symbol, range);

            return new BacktestData
            {
                Broker = task.Exchange.ToLower(),
                Pair = allocation.RobotSymbol ?? allocation.Symbol, // Get broker pair info: /admin/api/brokers/kucoin/pairs
                SourceFile = downloader.Download(task),
                Reverse = reverse,
                Balance = allocation.Balance,
                Start = start,
                Limit = limit,
                Offset = offset
            };
        }
    }
}