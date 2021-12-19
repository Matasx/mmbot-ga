using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;
using MMBotGA.backtest;
using MMBotGA.dto;
using Terminal.Gui;

namespace MMBotGA.ga.fitness
{
    class FitnessEvaluator : IFitness
    {
        private readonly ProgressBar _progressBar;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        private readonly IBacktest<ICollection<RunResponse>> _backtest;

        public FitnessEvaluator(ProgressBar progressBar, IBacktest<ICollection<RunResponse>> backtest)
        {
            _progressBar = progressBar;
            _backtest = backtest;
        }

        public double Evaluate(IChromosome chromosome)
            => EvaluateAsync(chromosome as StrategyChromosome).GetAwaiter().GetResult();

        private async Task<double> EvaluateAsync(StrategyChromosome chromosome)
        {
            var request = chromosome.ToBacktestRequest();
            
            chromosome.Metadata = "{{" + Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request.RunRequest.Config, _jsonOptions))) + "}}";

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = await _backtest.TestAsync(request);
            stopwatch.Stop();

            chromosome.Statistics = StatisticsEvaluator.Evaluate(request, result.Data);

            Application.MainLoop.Invoke(() =>
            {
                _progressBar.Pulse();
            });

            Console.WriteLine($"Done in {stopwatch.ElapsedMilliseconds} ms: {result.Fitness}");
            return result.Fitness;
        }
    }
}
