using CsvHelper;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using Downloader.Core.Core;

namespace MMBotGA
{
    class Program
    {
        static void Main(string[] args)
        {
            ServicePointManager.DefaultConnectionLimit = 10;

            var downloader = new DefaultDownloader();
            downloader.Download(new DownloadTask());
            var timeRange = DateTimeRange.FromUtcToday(TimeSpan.FromDays(-365));

            var apiPool = ApiDefinitions.GetLease();
            var backtest = new BacktestAggregator(new[] {
                new Backtest(apiPool, new BacktestData {
                    Broker = "kucoin",
                    Pair = "BTC-USDT", // Get broker pair info: /admin/api/brokers/kucoin/pairs
                    SourceFile = downloader.GetFile(new DownloadTask("KUCOIN", "BTC-USDT", timeRange)),
                    Reverse = false,
                    Balance = 10000
                }),
                new Backtest(apiPool, new BacktestData {
                    Broker = "kucoin",
                    Pair = "ETH-USDT",
                    SourceFile = downloader.GetFile(new DownloadTask("KUCOIN", "ETH-USDT", timeRange)),
                    Reverse = false,
                    Balance = 10000
                }),
                new Backtest(apiPool, new BacktestData {
                    Broker = "kucoin",
                    Pair = "ZEC-USDT",
                    SourceFile = downloader.GetFile(new DownloadTask("KUCOIN", "ZEC-USDT", timeRange)),
                    Reverse = false,
                    Balance = 10000
                }),
                new Backtest(apiPool, new BacktestData {
                    Broker = "binance",
                    Pair = "XRPBTC",
                    SourceFile = downloader.GetFile(new DownloadTask("BINANCE", "XRPBTC", timeRange)),
                    Reverse = false,
                    Balance = 0.1
                })
            });

            // GA
            var selection = new EliteSelection();
            var crossover = new UniformCrossover();
            var mutation = new UniformMutation(true);

            var fitness = new FitnessEvaluator(backtest);

            var chromosome = new StrategyChromosome();

            var population = new Population(500, 1000, chromosome);

            var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
            {
                Termination = new FitnessStagnationTermination(100),
                TaskExecutor = new ExactParallelTaskExecutor(apiPool.Available)
            };

            // Configure threadpool
            ThreadPool.GetMinThreads(out var minWorker, out var minIOC);
            ThreadPool.SetMinThreads(apiPool.Available, apiPool.Available);

            // Run & write results
            using var writer = new StreamWriter($"results-{DateTime.Now.ToString("s").Replace(':', '.')}.csv", false);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<CsvMap>();
            csv.WriteHeader<StrategyChromosome>();
            csv.NextRecord();
            csv.Flush();

            StrategyChromosome lastBest = null;

            ga.GenerationRan += (s, e) =>
            {
                var current = ga.BestChromosome as StrategyChromosome;
                if (current.Metadata != lastBest?.Metadata)
                {
                    lastBest = current;
                    csv.WriteRecord(lastBest);
                    csv.NextRecord();
                    csv.Flush();
                }

                Console.WriteLine();
                Console.WriteLine($"Generation {ga.GenerationsNumber}. Best fitness: {ga.BestChromosome.Fitness.Value}");
                Console.WriteLine();
            };

            Console.WriteLine("GA running...");
            ga.Start();
        }
    }
}
