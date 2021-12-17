using GeneticSharp.Domain;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Downloader.Core.Core;
using MMBotGA.dto;

namespace MMBotGA
{
    internal class Program
    {
        private static void Main()
        {
            ServicePointManager.DefaultConnectionLimit = 10;

            var apiPool = ApiDefinitions.GetLease();
            ThreadPool.SetMinThreads(apiPool.Available, apiPool.Available);

            var downloader = new DefaultDownloader();
            var timeRange = DateTimeRange.FromUtcToday(TimeSpan.FromDays(-365));

            var batches = new[]
            {
                new Batch("BTC-USDT", new[]
                {
                    downloader.GetBacktestData(new DownloadTask("KUCOIN", "BTC-USDT", timeRange), false, 10000),
                    downloader.GetBacktestData(new DownloadTask("KUCOIN", "BTC-USDT", timeRange), true, 10000)
                }),
                new Batch("ETH-USDT", new[]
                {
                    downloader.GetBacktestData(new DownloadTask("KUCOIN", "ETH-USDT", timeRange), false, 10000),
                    downloader.GetBacktestData(new DownloadTask("KUCOIN", "ETH-USDT", timeRange), true, 10000)
                }),
                new Batch("ZEC-USDT", new[]
                {
                    downloader.GetBacktestData(new DownloadTask("KUCOIN", "ZEC-USDT", timeRange), false, 10000),
                    downloader.GetBacktestData(new DownloadTask("KUCOIN", "ZEC-USDT", timeRange), true, 10000)
                }),
                new Batch("BNB-USDT", new[]
                {
                    downloader.GetBacktestData(new DownloadTask("BINANCE", "BNBUSDT", timeRange), false, 10000),
                    downloader.GetBacktestData(new DownloadTask("BINANCE", "BNBUSDT", timeRange), true, 10000)
                }),
                new Batch("BNB-EUR", new[]
                {
                    downloader.GetBacktestData(new DownloadTask("BINANCE", "BNBEUR", timeRange), false, 10000),
                    downloader.GetBacktestData(new DownloadTask("BINANCE", "BNBEUR", timeRange), true, 10000)
                }),
                new Batch("SOL-EUR", new[]
                {
                    downloader.GetBacktestData(new DownloadTask("BINANCE", "SOLEUR", timeRange), false, 10000),
                    downloader.GetBacktestData(new DownloadTask("BINANCE", "SOLEUR", timeRange), true, 10000)
                }),
                new Batch("ADA-USDT", new[]
                {
                    downloader.GetBacktestData(new DownloadTask("BINANCE", "ADAUSDT", timeRange), false, 10000),
                    downloader.GetBacktestData(new DownloadTask("BINANCE", "ADAUSDT", timeRange), true, 10000)
                })
            };

            // GA
            var selection = new EliteSelection();
            var crossover = new UniformCrossover();
            var mutation = new UniformMutation(true);
            var chromosome = new StrategyChromosome();
            var population = new Population(500, 1000, chromosome);
            var termination = new FitnessStagnationTermination(15);
            var executor = new ExactParallelTaskExecutor(apiPool.Available);

            using var csv = new CsvWrapper<CsvMap, StrategyChromosome>("MASTER");
            foreach (var batch in batches)
            {
                var backtestData = batch.BacktestData
                    .Select(x => new Backtest(apiPool, x))
                    .Cast<IBacktest<ICollection<RunResponse>>>()
                    .ToList();
                var backtest = new BacktestAggregator<ICollection<RunResponse>>(backtestData);
                var fitness = new FitnessEvaluator(backtest);
                var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
                {
                    Termination = termination,
                    TaskExecutor = executor
                };

                var best = Run(ga, batch.Name);
                if (best != null) csv.WriteRecord(best);
            }
        }

        private static StrategyChromosome Run(GeneticAlgorithm ga, string name)
        {
            using var csv = new CsvWrapper<CsvMap, StrategyChromosome>(name);

            StrategyChromosome lastBest = null;

            void OnGenerationRan(object o, EventArgs eventArgs)
            {
                var current = ga.BestChromosome as StrategyChromosome;
                if (current.Metadata != lastBest?.Metadata)
                {
                    lastBest = current;
                    lastBest.ID = name;
                    lastBest.Generation = ga.GenerationsNumber;
                    csv.WriteRecord(lastBest);
                }

                Console.WriteLine();
                Console.WriteLine($"Generation {ga.GenerationsNumber}. Best fitness: {ga.BestChromosome.Fitness.Value}");
                Console.WriteLine();
            }

            ga.GenerationRan += OnGenerationRan;

            Console.WriteLine("GA running...");
            ga.Start();

            ga.GenerationRan -= OnGenerationRan;
            return lastBest;
        }
    }
}
