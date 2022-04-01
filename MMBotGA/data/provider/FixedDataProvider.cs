﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Downloader.Core.Core;
using MMBotGA.backtest;
using MMBotGA.data.exchange;
using MMBotGA.downloader;
using MMBotGA.ga;
using MMBotGA.ga.abstraction;
using Newtonsoft.Json;

namespace MMBotGA.data.provider
{
    internal class FixedDataProvider : IDataProvider
    {
        private const string DataFolder = "data";

        private int lookBackBacktestDays = -365; //How far do we backtest back?
        private int lookBackControlDays = -60; //How far is the control set?

        protected virtual DataProviderSettings Settings => new()
        {
            Allocations = AllocationDefinitions.Select(x => x.ToAllocation()).ToArray(),
            DateSettings = new DataProviderDateSettings
            {
                Automatic = true,
                Backtest = DateTimeRange.FromDiff(DateTime.UtcNow.Date, TimeSpan.FromDays(lookBackBacktestDays)),
                Control = DateTimeRange.FromDiff(DateTime.UtcNow.Date, TimeSpan.FromDays(lookBackControlDays))
            }
        };

        private static IEnumerable<AllocationDefinition> AllocationDefinitions => new AllocationDefinition[]
        {
            new()
            {
                Exchange = Exchange.Ftx,
                Pair = new Pair("FTM", "PERP"),
                Balance = 1000
            },
            //new()
            //{
            //    Exchange = Exchange.Ftx,
            //    Pair = new Pair("FTM", "PERP"),
            //    Balance = 1000
            //},
            //EpaChromozom - Prepnuti GA do EPA.
            //new()
            //{
            //    Exchange = Exchange.Kucoin,
            //    Pair = new Pair("DGTX", "BTC"),
            //    Balance = 0.01,
            //    // Set strategy manually and train just spread
            //    AdamChromosome = new SpreadChromosome(new dto.EpaStrategy
            //    {
            //        Type = "enter_price_angle",
            //        MinAssetPercOfBudget = 0.001d,
            //        InitialBetPercOfBudget = 0.01d,
            //        MaxEnterPriceDistance = 0.03d,
            //        PowerMult = 1,
            //        PowerCap = 1,
            //        Angle = 35,
            //        TargetExitPriceDistance = 0.01d,
            //        ExitPowerMult = 1,
            //        Backtest = false
            //    })
            //},
            //GammaChromozom - Prepnuti GA do Gamma.
            //new()
            //{
            //    Exchange = Exchange.Bitfinex,
            //    Pair = new Pair("ZEC", "USD"),
            //    Balance = 1000,
            //    // Set strategy manually and train just spread
            //    AdamChromosome = new SpreadChromosome(new dto.GammaStrategy
            //    {
            //        Type = "gamma",
            //        Exponent = 7,
            //        Trend = -70,
            //        Function = "gauss",
            //        Rebalance = "3",
            //        Reinvest = false
            //    })
            //},
            //DcaChromozom - Prepnuti GA do DCA.
            //new ()
            //{
            //    Exchange = Exchange.Kucoin,
            //    Pair = new Pair("DGTX", "BTC"),
            //    Balance = 1000,
            //    // Set strategy manually and train just spread
            //    AdamChromosome = new SpreadChromosome(new dto.DcaStrategy
            //    {
            //        Type = "inc_value",
            //        Reduction = 0.5d,
            //        Power = 3d,
            //        Exponent = 5d,
            //        MaxSpread = 0d
            //    })
            //},
            //
        };

        public Batch[] GetBacktestData(IProgress progressCallback)
        {
            File.WriteAllText("allocations.json.sample", JsonConvert.SerializeObject(Settings, Formatting.Indented)); 

            var downloader = new DefaultDownloader(progressCallback);
            var backtestRange = Settings.DateSettings.Automatic
                ? DateTimeRange.FromDiff(DateTime.UtcNow.Date.AddDays(0), TimeSpan.FromDays(lookBackBacktestDays))
                : Settings.DateSettings.Backtest;

            const int splits = 3; // 3

            return Settings.Allocations
                .Select(x =>
                {
                    var file = downloader.Download(new DownloadTask(DataFolder, x.Exchange, x.Symbol, backtestRange));
                    return new
                    {
                        Allocation = x,
                        File = file,
                        Size = File.ReadAllLines(file).Length
                    };
                })
                .Where(x => x.Size > 100)
                .Select(x =>
                {
                    var partMinutes = x.Size / splits;
                    var halfPartMinutes = partMinutes / 2;

                    var offsets = Enumerable
                        .Repeat(partMinutes, splits)
                        .Select((p, i) => p * i)
                        .Concat(Enumerable
                            .Repeat(partMinutes, splits - 1)
                            .Select((p, i) => halfPartMinutes + p * i)
                        );

                    return new Batch(x.Allocation.ToBatchName(), x.Allocation.AdamChromosome,
                        offsets
                            .Select(o => new BacktestData
                            {
                                Broker = x.Allocation.Exchange.ToLower(),
                                Pair = x.Allocation.RobotSymbol ??
                                       x.Allocation.Symbol, // Get broker pair info: /admin/api/brokers/kucoin/pairs
                                SourceFile = x.File,
                                Reverse = false,
                                Balance = x.Allocation.Balance,
                                Start = null,
                                Limit = partMinutes,
                                Offset = o
                            })
                            .ToArray()
                    );
                })
                .ToArray();
        }

        public Batch[] GetControlData(IProgress progressCallback)
        {
            var downloader = new DefaultDownloader(progressCallback);
            var controlRange = Settings.DateSettings.Automatic
                ? DateTimeRange.FromUtcToday(TimeSpan.FromDays(lookBackControlDays))
                : Settings.DateSettings.Control;

            return Settings.Allocations
                .Select(x => new Batch(x.ToBatchName(), x.AdamChromosome,
                    new[]
                    {
                        downloader.GetBacktestData(x, DataFolder, controlRange, false)
                    }))
                .ToArray();
        }
    }
}