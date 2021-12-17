using System.Collections.Generic;
using System.Linq;
using GeneticSharp.Domain.Fitnesses;
using MMBotGA.dto;

namespace MMBotGA;

internal static class BatchExtensions
{
    public static IFitness ToFitness(this Batch batch, ApiLease apiPool)
    {
        var backtestData = batch.BacktestData
            .Select(x => new Backtest(apiPool, x))
            .Cast<IBacktest<ICollection<RunResponse>>>()
            .ToList();
        var backtest = new BacktestAggregator<ICollection<RunResponse>>(backtestData);
        return new FitnessEvaluator(backtest);
    }
}