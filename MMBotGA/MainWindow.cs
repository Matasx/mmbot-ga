using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Downloader.Core.Core;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using Terminal.Gui;

namespace MMBotGA;

internal class MainWindow
{
    private readonly TextField _txtBatch;
    private readonly TextField _txtGeneration;
    private readonly TextField _txtFitness;
    private readonly ProgressDialog _progressDialog;
    private readonly ProgressBar _progressBar;

    private readonly DefaultDownloader _downloader;

    public MainWindow(Toplevel top)
    {
        var window = new Window("MMBot GA")
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        top.Add(window);

        var menu = new MenuBar(new MenuBarItem[]
        {
            new("_File", new MenuItem[]
            {
                new("_Quit", "", () =>
                {
                    if (Quit()) top.Running = false;
                })
            })
        });
        top.Add(menu);

        var lblbatch = new Label("Batch: ");
        _txtBatch = new TextField
        {
            ReadOnly = true,
            X = Pos.Right(lblbatch),
            Y = Pos.Top(lblbatch),
            Width = 10
        };

        var lblGeneration = new Label("Generation: ")
        {
            Y = Pos.Bottom(lblbatch)
        };
        _txtGeneration = new TextField
        {
            ReadOnly = true,
            X = Pos.Right(lblGeneration),
            Y = Pos.Top(lblGeneration),
            Width = 10
        };

        var lblFitness = new Label("Best fitness: ")
        {
            Y = Pos.Bottom(lblGeneration)
        };
        _txtFitness = new TextField
        {
            ReadOnly = true,
            X = Pos.Right(lblFitness),
            Y = Pos.Top(lblFitness),
            Width = 10
        };

        _progressBar = new ProgressBar
        {
            Y = Pos.Bottom(lblFitness),
            Width = Dim.Fill()
        };

        window.Add(lblGeneration, _txtGeneration, lblFitness, _txtFitness, lblbatch, _txtBatch, _progressBar);

        _progressDialog = new ProgressDialog(window);
        _downloader = new DefaultDownloader(_progressDialog);
    }

    private static bool Quit()
    {
        var n = MessageBox.Query(50, 7, "Quit", "Are you sure you want to quit?", "Yes", "No");
        return n == 0;
    }

    public Task RunTask()
    {
        return Task.Run(Run);
    }

    public void Run()
    {
        var apiPool = ApiDefinitions.GetLease();
        ThreadPool.SetMinThreads(apiPool.Available, apiPool.Available);

        var backtestBatches = GetBatches(DateTimeRange.FromDiff(DateTime.UtcNow.Date.AddDays(-60), TimeSpan.FromDays(-365)));
        var controlBatches = GetBatches(DateTimeRange.FromUtcToday(TimeSpan.FromDays(-60)));

        _progressDialog.Hide();

        // GA
        var selection = new EliteSelection();
        var crossover = new UniformCrossover();
        var mutation = new UniformMutation(true);
        var chromosome = new StrategyChromosome();
        var population = new Population(500, 1000, chromosome);
        var termination = new FitnessStagnationTermination(15);
        var executor = new ExactParallelTaskExecutor(apiPool.Available);

        using var csvBacktest = new CsvWrapper<CsvMap, StrategyChromosome>("BACKTEST");
        using var csvControl = new CsvWrapper<CsvMap, StrategyChromosome>("CONTROL");
        foreach (var batch in backtestBatches)
        {
            Application.MainLoop.Invoke(() => { _txtBatch.Text = batch.Name; });

            var ga = new GeneticAlgorithm(population, batch.ToFitness(_progressBar, apiPool), selection, crossover, mutation)
            {
                Termination = termination,
                TaskExecutor = executor
            };

            var best = RunGA(ga, batch.Name);
            if (best != null)
            {
                csvBacktest.WriteRecord(best);

                // Re-evaluate over control set
                var controlFitness = controlBatches.First(x => x.Name == batch.Name).ToFitness(_progressBar, apiPool);
                controlFitness.Evaluate(best);
                csvControl.WriteRecord(best);
            }
        }

        Application.MainLoop.Invoke(() => MessageBox.Query("Information", "GA is finished.", "OK"));
    }

    private Batch[] GetBatches(DateTimeRange timeRange)
    {
        return new[]
        {
            new Batch("BTC-USDT", new[]
            {
                _downloader.GetBacktestData(new DownloadTask("KUCOIN", "BTC-USDT", timeRange), false, 10000),
                _downloader.GetBacktestData(new DownloadTask("KUCOIN", "BTC-USDT", timeRange), true, 10000)
            }),
            new Batch("ETH-USDT", new[]
            {
                _downloader.GetBacktestData(new DownloadTask("KUCOIN", "ETH-USDT", timeRange), false, 10000),
                _downloader.GetBacktestData(new DownloadTask("KUCOIN", "ETH-USDT", timeRange), true, 10000)
            }),
            new Batch("ZEC-USDT", new[]
            {
                _downloader.GetBacktestData(new DownloadTask("KUCOIN", "ZEC-USDT", timeRange), false, 10000),
                _downloader.GetBacktestData(new DownloadTask("KUCOIN", "ZEC-USDT", timeRange), true, 10000)
            }),
            new Batch("BNB-USDT", new[]
            {
                _downloader.GetBacktestData(new DownloadTask("BINANCE", "BNBUSDT", timeRange), false, 10000),
                _downloader.GetBacktestData(new DownloadTask("BINANCE", "BNBUSDT", timeRange), true, 10000)
            }),
            new Batch("BNB-EUR", new[]
            {
                _downloader.GetBacktestData(new DownloadTask("BINANCE", "BNBEUR", timeRange), false, 10000),
                _downloader.GetBacktestData(new DownloadTask("BINANCE", "BNBEUR", timeRange), true, 10000)
            }),
            new Batch("SOL-EUR", new[]
            {
                _downloader.GetBacktestData(new DownloadTask("BINANCE", "SOLEUR", timeRange), false, 10000),
                _downloader.GetBacktestData(new DownloadTask("BINANCE", "SOLEUR", timeRange), true, 10000)
            }),
            new Batch("ADA-USDT", new[]
            {
                _downloader.GetBacktestData(new DownloadTask("BINANCE", "ADAUSDT", timeRange), false, 10000),
                _downloader.GetBacktestData(new DownloadTask("BINANCE", "ADAUSDT", timeRange), true, 10000)
            })
        };
    }

    private StrategyChromosome RunGA(GeneticAlgorithm ga, string name)
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

            Application.MainLoop.Invoke(() =>
            {
                _txtGeneration.Text = ga.GenerationsNumber.ToString();
                _txtFitness.Text = ga.BestChromosome.Fitness.Value.ToString();
            });
        }

        ga.GenerationRan += OnGenerationRan;
        ga.Start();
        ga.GenerationRan -= OnGenerationRan;
        return lastBest;
    }
}