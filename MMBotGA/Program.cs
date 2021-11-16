using CsvHelper;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace MMBotGA
{
    class Program
    {
        static void Main(string[] args)
        {
            static LeasableApi CreateBackend(int leaseCount, string url, string username, string password)
            {
                return new LeasableApi(leaseCount, new Api(url, new HttpClient(new HttpClientHandler()
                {
                    Credentials = new NetworkCredential(username, password),
                    PreAuthenticate = true,
                    MaxConnectionsPerServer = 20
                })));
            }

            var apiPool = new ApiLease(
                //RPi
                CreateBackend(6, "http://192.168.1.150:10000/admin/api/", "user", "pass"),
                //mtxs
                CreateBackend(10, "http://192.168.1.170:20000/admin/api/", "user", "pass")
            );
            var backtest = new BacktestAggregator(new[] {
                new Backtest(apiPool, new BacktestData {
                    Broker = "kucoin",
                    Pair = "BTC-USDT", // Get broker pair info: /admin/api/brokers/kucoin/pairs
                    SourceFile = "data\\BTCUSDT_1.1.2021_11.11.2021.csv",
                    Reverse = false,
                    Balance = 10000
                }),
                new Backtest(apiPool, new BacktestData {
                    Broker = "kucoin",
                    Pair = "ETH-USDT",
                    SourceFile = "data\\ETHUSDT_1.1.2021_11.11.2021.csv",
                    Reverse = false,
                    Balance = 10000
                }),
                new Backtest(apiPool, new BacktestData {
                    Broker = "kucoin",
                    Pair = "ZEC-USDT",
                    SourceFile = "data\\ZECUSDT_1.1.2021_11.11.2021.csv",
                    Reverse = false,
                    Balance = 10000
                }),
                new Backtest(apiPool, new BacktestData {
                    Broker = "binance",
                    Pair = "XRPBTC",
                    SourceFile = "data\\XRPBTC_1.1.2021_11.11.2021.csv",
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

            IChromosome lastBest = null;

            ga.GenerationRan += (s, e) =>
            {
                if (ga.BestChromosome != lastBest)
                {
                    lastBest = ga.BestChromosome;
                    csv.WriteRecord(lastBest as StrategyChromosome);
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
