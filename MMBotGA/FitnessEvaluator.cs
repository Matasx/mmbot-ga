using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;
using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MMBotGA
{
    class FitnessEvaluator : IFitness
    {
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        private readonly IBacktest _backtest;

        public FitnessEvaluator(IBacktest backtest)
        {
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

            Console.WriteLine($"Done in {stopwatch.ElapsedMilliseconds} ms: {result}");
            return result;
        }
    }
}
