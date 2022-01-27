using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using MMBotGA.api;
using MMBotGA.data.provider;
using MMBotGA.ga;
using MMBotGA.ga.abstraction;
using MMBotGA.ga.execution;
using MMBotGA.io;
using MMBotGA.ui;
using Terminal.Gui;

namespace MMBotGA
{
    internal class MainWindow
    {
        private readonly TextField _txtBatch;
        private readonly TextField _txtGeneration;
        private readonly TextField _txtFitness;
        private readonly ProgressDialog _progressDialog;
        private readonly ProgressBar _totalProgressBar;
        private readonly ProgressBar _currentProgressBar;

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

            var lblBatch = new Label("Batch: ")
            {
                Width = 15
            };
            _txtBatch = new TextField
            {
                ReadOnly = true,
                X = Pos.Right(lblBatch),
                Y = Pos.Top(lblBatch),
                Width = 30
            };

            var lblGeneration = new Label("Generation: ")
            {
                Y = Pos.Bottom(lblBatch)
            };
            _txtGeneration = new TextField
            {
                ReadOnly = true,
                X = _txtBatch.X,
                Y = Pos.Top(lblGeneration),
                Width = _txtBatch.Width
            };

            var lblFitness = new Label("Best fitness: ")
            {
                Y = Pos.Bottom(lblGeneration)
            };
            _txtFitness = new TextField
            {
                ReadOnly = true,
                X = _txtBatch.X,
                Y = Pos.Top(lblFitness),
                Width = _txtBatch.Width
            };

            _totalProgressBar = new ProgressBar
            {
                Y = Pos.Bottom(lblFitness) + 1,
                Width = Dim.Fill()
            };
            _currentProgressBar = new ProgressBar
            {
                Y = Pos.Bottom(_totalProgressBar),
                Width = Dim.Fill()
            };

            window.Add(lblGeneration, _txtGeneration, lblFitness, _txtFitness, lblBatch, _txtBatch, _totalProgressBar,
                _currentProgressBar);

            _progressDialog = new ProgressDialog(window);
        }

        private static bool Quit()
        {
            var n = MessageBox.Query(50, 7, "Quit", "Are you sure you want to quit?", "Yes", "No");
            return n == 0;
        }

        public void Run()
        {
            var apiPool = ApiDefinitions.GetLease();
            ThreadPool.SetMinThreads(apiPool.Available, apiPool.Available);

            //TODO: pick your data provider
            var dataProvider = new FixedDataProvider();
            //var dataProvider = new JsonConfiguredDataProvider();

            var backtestBatches = dataProvider.GetBacktestData(_progressDialog);
            var controlBatches = dataProvider.GetControlData(_progressDialog); 

            _progressDialog.Hide();

            // GA
            var selection = new EliteSelection();
            var crossover = new UniformCrossover();
            var mutation = new UniformMutation(true);
            var termination = new FitnessStagnationTermination(40);
            var executor = new ExactParallelTaskExecutor(apiPool.Available);

            var csvHandlers = new Dictionary<Tuple<Type, Type>, CsvWrapper>();
            var current = 0;
            var csvIndex = 0;
            foreach (var batch in backtestBatches)
            {
                current++;
                Application.MainLoop.Invoke(() =>
                {
                    _txtBatch.Text = batch.Name;
                    _txtGeneration.Text = "0";
                    _txtFitness.Text = string.Empty;
                });

                var population = new Population(500, 3000, batch.AdamChromosome);
                var ga = new GeneticAlgorithm(population, batch.ToFitness(_currentProgressBar, apiPool),
                    selection, crossover,
                    mutation)
                {
                    Termination = termination,
                    TaskExecutor = executor
                };

                var best = RunGA(ga, batch.AdamChromosome, batch.Name);
                var progress = (float) current / backtestBatches.Length;
                Application.MainLoop.Invoke(() => { _totalProgressBar.Fraction = progress; });

                if (best == null) continue;
                best.BacktestStats = best.Statistics;

                // Re-evaluate over control set
                var controlFitness = controlBatches
                    .FirstOrDefault(x => x.Name == batch.Name)
                    ?.ToFitness(_currentProgressBar, apiPool);
                if (controlFitness != null)
                {
                    controlFitness.Evaluate(best);
                    best.ControlStats = best.Statistics;
                }

                var key = new Tuple<Type, Type>(batch.AdamChromosome.CsvAggregatedMapType, batch.AdamChromosome.CsvRecordType);
                if (!csvHandlers.TryGetValue(key, out var csvWrapper))
                {
                    csvHandlers[key] = csvWrapper = new CsvWrapper($"MASTER-{++csvIndex}", key.Item1, key.Item2);
                }
                csvWrapper.WriteRecord(best);
            }

            foreach (var csv in csvHandlers.Values)
            {
                csv.Dispose();
            }

            Application.MainLoop.Invoke(() =>
            {
                _txtBatch.Text = "FINISHED";
                _txtGeneration.Text = string.Empty;
                _txtFitness.Text = string.Empty;
            });

            //Application.MainLoop.Invoke(() => MessageBox.Query("Information", "GA is finished.", "OK"));
        }

        private ICustomChromosome RunGA(GeneticAlgorithm ga, ICustomChromosome adamChromosome, string name)
        {
            using var csv = new CsvWrapper(name, adamChromosome.CsvSingleMapType, adamChromosome.CsvRecordType);

            ICustomChromosome lastBest = null;

            void OnGenerationRan(object o, EventArgs eventArgs)
            {
                var current = ga.BestChromosome as ICustomChromosome;
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
}