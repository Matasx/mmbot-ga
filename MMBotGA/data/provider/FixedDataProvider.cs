using System;
using System.Collections.Generic;
using System.Linq;
using Downloader.Core.Core;
using MMBotGA.data.exchange;
using MMBotGA.downloader;
using MMBotGA.ga.abstraction;

namespace MMBotGA.data.provider
{
    internal class FixedDataProvider : IDataProvider
    {
        private static IEnumerable<AllocationDefinition> AllocationDefinitions => new AllocationDefinition[]
        {
            new()
            {
                Exchange = Exchange.Kucoin,
                Pair = new Pair("BTC", "USDT"),
                Balance = 10000
            },
            new()
            {
                Exchange = Exchange.Kucoin,
                Pair = new Pair("ETH", "USDT"),
                Balance = 10000
            },
            new()
            {
                Exchange = Exchange.Kucoin,
                Pair = new Pair("ZEC", "USDT"),
                Balance = 10000
            },
            new()
            {
                Exchange = Exchange.Kucoin,
                Pair = new Pair("XRP", "USDT"),
                Balance = 10000
            },
            new()
            {
                Exchange = Exchange.Kucoin,
                Pair = new Pair("FTM", "USDT"),
                Balance = 10000
            },
            new()
            {
                Exchange = Exchange.Kucoin,
                Pair = new Pair("LTC", "USDT"),
                Balance = 10000
            },
            new()
            {
                Exchange = Exchange.Kucoin,
                Pair = new Pair("FLUX", "USDT"),
                Balance = 10000
            },
            new()
            {
                Exchange = Exchange.Binance,
                Pair = new Pair("AVAX", "USDT"),
                Balance = 10000
            },
            new()
            {
                Exchange = Exchange.Binance,
                Pair = new Pair("BNB", "USDT"),
                Balance = 10000
            },
            new()
            {
                Exchange = Exchange.Binance,
                Pair = new Pair("SOL", "USDT"),
                Balance = 10000
            },
            new()
            {
                Exchange = Exchange.Binance,
                Pair = new Pair("ADA", "USDT"),
                Balance = 10000
            }
        };

        protected virtual IEnumerable<Allocation> Allocations => AllocationDefinitions.Select(x => x.ToAllocation());

        public Batch[] GetBacktestData(IProgress progressCallback)
        {
            //File.WriteAllText("allocations.json", JsonConvert.SerializeObject(Allocations, Formatting.Indented)); 

            var downloader = new DefaultDownloader(progressCallback);
            var backtestRange = DateTimeRange.FromDiff(DateTime.UtcNow.Date.AddDays(-60), TimeSpan.FromDays(-365));

            return Allocations
                .Select(x => new Batch(x.ToBatchName(),
                    new[]
                    {
                        downloader.GetBacktestData(new DownloadTask(x.Exchange, x.Symbol, backtestRange), false, x.Balance),
                        downloader.GetBacktestData(new DownloadTask(x.Exchange, x.Symbol, backtestRange), true, x.Balance)
                    }))
                .ToArray();
        }

        public Batch[] GetControlData(IProgress progressCallback)
        {
            var downloader = new DefaultDownloader(progressCallback);
            var backtestRange = DateTimeRange.FromUtcToday(TimeSpan.FromDays(-60));

            return Allocations
                .Select(x => new Batch(x.ToBatchName(),
                    new[]
                    {
                        downloader.GetBacktestData(new DownloadTask(x.Exchange, x.Symbol, backtestRange), false, x.Balance)
                    }))
                .ToArray();
        }
    }
}