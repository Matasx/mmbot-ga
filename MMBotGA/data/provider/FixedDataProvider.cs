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
        private const string DataFolder = "data";

        protected virtual DataProviderSettings Settings => new()
        {
            Allocations = AllocationDefinitions.Select(x => x.ToAllocation()).ToArray(),
            DateSettings = new DataProviderDateSettings
            {
                Automatic = true,
                Backtest = DateTimeRange.FromDiff(DateTime.UtcNow.Date.AddDays(-60), TimeSpan.FromDays(-365)),
                Control = DateTimeRange.FromDiff(new DateTime(2022, 1, 4, 0,0,0, DateTimeKind.Utc), TimeSpan.FromDays(-60))
            }
        };

        private static IEnumerable<AllocationDefinition> AllocationDefinitions => new AllocationDefinition[]
        {
            new()
            {
                Exchange = Exchange.Kucoin,
                Pair = new Pair("ZEC", "USDT"),
                Balance = 1000
            },
            //new()
            //{
            //    Exchange = Exchange.Kucoin,
            //    Pair = new Pair("FLUX", "USDT"),
            //    Balance = 1000
            //},
            //new()
            //{
            //    Exchange = Exchange.Kucoin,
            //    Pair = new Pair("HTR", "USDT"),
            //    Balance = 1000
            //},
            //new()
            //{
            //    Exchange = Exchange.Binance,
            //    Pair = new Pair("LSK", "USDT"),
            //    Balance = 1000
            //},
            //new()
            //{
            //    Exchange = Exchange.Kucoin,
            //    Pair = new Pair("BTC", "USDT"),
            //    Balance = 1000
            //},
            //new()
            //{
            //    Exchange = Exchange.Kucoin,
            //    Pair = new Pair("ETH", "USDT"),
            //    Balance = 1000
            //},
            //new()
            //{
            //    Exchange = Exchange.Bitfinex,
            //    Pair = new Pair("ZEC", "USD"),
            //    Balance = 1000
            //},
            //new()
            //{
            //    Exchange = Exchange.Kucoin,
            //    Pair = new Pair("XRP", "USDT"),
            //    Balance = 1000
            //},
            //new()
            //{
            //    Exchange = Exchange.Kucoin,
            //    Pair = new Pair("FTM", "USDT"),
            //    Balance = 1000
            //},
            //new()
            //{
            //    Exchange = Exchange.Kucoin,
            //    Pair = new Pair("LTC", "USDT"),
            //    Balance = 1000
            //},
            //new()
            //{
            //    Exchange = Exchange.Kucoin,
            //    Pair = new Pair("FLUX", "USDT"),
            //    Balance = 1000
            //},
            //new()
            //{
            //    Exchange = Exchange.Binance,
            //    Pair = new Pair("AVAX", "USDT"),
            //    Balance = 1000
            //},
            //new()
            //{
            //    Exchange = Exchange.Binance,
            //    Pair = new Pair("BNB", "USDT"),
            //    Balance = 1000
            //},
            //new()
            //{
            //    Exchange = Exchange.Binance,
            //    Pair = new Pair("SOL", "USDT"),
            //    Balance = 1000
            //},
            //new()
            //{
            //    Exchange = Exchange.Binance,
            //    Pair = new Pair("ADA", "USDT"),
            //    Balance = 1000
            //}
        };

        public Batch[] GetBacktestData(IProgress progressCallback)
        {
            //File.WriteAllText("allocations.json", JsonConvert.SerializeObject(Settings, Formatting.Indented)); 

            var downloader = new DefaultDownloader(progressCallback);
            var backtestRange = Settings.DateSettings.Automatic
                ? DateTimeRange.FromDiff(DateTime.UtcNow.Date.AddDays(-60), TimeSpan.FromDays(-365))
                : Settings.DateSettings.Backtest;

            const int splits = 3;
            var diff = backtestRange.End - backtestRange.Start;
            var partMinutes = (int)diff.TotalMinutes / splits;
            var halfPartMinutes = partMinutes / 2;

            var offsets = Enumerable
                .Repeat(partMinutes, splits)
                .Select((p, i) => p * i)
                .Concat(Enumerable
                    .Repeat(partMinutes, splits - 1)
                    .Select((p, i) => halfPartMinutes + p * i)
                );

            return Settings.Allocations
                .Select(x => new Batch(x.ToBatchName(),
                    offsets
                        .Select(o => downloader.GetBacktestData(x, DataFolder, backtestRange, false, limit: partMinutes, offset: o))
                        .ToArray()
                ))
                .ToArray();

            //var partDays = (int)diff.TotalDays / splits;
            //var overlapStart = backtestRange.Start.AddDays(partDays / 2);
            //var parts = Enumerable
            //    .Repeat(partDays, splits)
            //    .Select((p, i) => DateTimeRange.FromDiff(backtestRange.Start.AddDays(p * i), TimeSpan.FromDays(p)))
            //    .Concat(Enumerable
            //        .Repeat(partDays, splits - 1)
            //        .Select((p, i) => DateTimeRange.FromDiff(overlapStart.AddDays(p * i), TimeSpan.FromDays(p)))
            //    );

            //return Settings.Allocations
            //    .Select(x => new Batch(x.ToBatchName(),
            //        new[]
            //        {
            //            downloader.GetBacktestData(x, DataFolder, backtestRange, false),
            //            downloader.GetBacktestData(x, DataFolder, backtestRange, true)
            //        }))
            //    .ToArray();
        }

        public Batch[] GetControlData(IProgress progressCallback)
        {
            var downloader = new DefaultDownloader(progressCallback);
            var controlRange = Settings.DateSettings.Automatic
                ? DateTimeRange.FromUtcToday(TimeSpan.FromDays(-60))
                : Settings.DateSettings.Control;

            return Settings.Allocations
                .Select(x => new Batch(x.ToBatchName(),
                    new[]
                    {
                        downloader.GetBacktestData(x, DataFolder, controlRange, false)
                    }))
                .ToArray();
        }
    }
}