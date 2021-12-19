using System.Collections.Generic;
using System.Linq;
using GeneticSharp.Domain.Fitnesses;
using MMBotGA.api;
using MMBotGA.backtest;
using MMBotGA.dto;
using MMBotGA.ga.fitness;
using Terminal.Gui;

namespace MMBotGA.ga.abstraction;

internal static class BatchExtensions
{
    public static IFitness ToFitness(this Batch batch, ProgressBar progressBar, ApiLease apiPool)
    {
        var backtestData = batch.BacktestData
            .Select(x => new Backtest(apiPool, x))
            .Cast<IBacktest<ICollection<RunResponse>>>()
            .ToList();
        var backtest = new BacktestAggregator<ICollection<RunResponse>>(backtestData);
        return new FitnessEvaluator(progressBar, backtest);
    }
}