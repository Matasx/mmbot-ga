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
using System.Linq;
using System.Net;
using System.Threading;
using Downloader.Core.Core;

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
                    downloader.GetBacktestData(new DownloadTask("KUCOIN", "ETH-USDT", timeRange), false, 10000),
                    downloader.GetBacktestData(new DownloadTask("KUCOIN", "ZEC-USDT", timeRange), false, 10000),
                    downloader.GetBacktestData(new DownloadTask("BINANCE", "XRPBTC", timeRange), false, 0.1)
                })
            };

            // GA
            var selection = new EliteSelection();
            var crossover = new UniformCrossover();
            var mutation = new UniformMutation(true);
            var chromosome = new StrategyChromosome();
            var population = new Population(500, 1000, chromosome);
            var termination = new FitnessStagnationTermination(30);
            var executor = new ExactParallelTaskExecutor(apiPool.Available);

            foreach (var batch in batches)
            {
                var backtestData = batch.BacktestData
                    .Select(x => new Backtest(apiPool, x))
                    .Cast<IBacktest>()
                    .ToList();
                var backtest = new BacktestAggregator(backtestData);
                var fitness = new FitnessEvaluator(backtest);
                var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
                {
                    Termination = termination,
                    TaskExecutor = executor
                };
                //TODO: csv results per batch
                Run(ga, batch.Name);
            }
        }

        private static void Run(GeneticAlgorithm ga, string name)
        {
            using var csv = new CsvWrapper<CsvMap, StrategyChromosome>(name);

            StrategyChromosome lastBest = null;

            void OnGenerationRan(object o, EventArgs eventArgs)
            {
                var current = ga.BestChromosome as StrategyChromosome;
                if (current.Metadata != lastBest?.Metadata)
                {
                    lastBest = current;
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
        }
    }
}
