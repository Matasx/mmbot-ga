using System;
using System.Collections.Generic;
using System.Linq;
using MMBotGA.backtest;
using MMBotGA.dto;

namespace MMBotGA.ga.fitness
{
    internal static class EpaFitnessFunctions
    {
        private static double EvaluateFit(
            BacktestRequest request,
            ICollection<RunResponse> results
            )
        {
            // continuity -> stable performance and delivery of budget extra
            // get profit at least every x days

            var firstTime = results.First().Tm;
            var timeFrame = results.Last().Tm - firstTime;
            var frames = (int)(TimeSpan.FromMilliseconds(timeFrame).TotalDays / 25);
            if (frames == 0) return 0;
            var gk = timeFrame / frames;
            var lastBudgetExtra = 0d;
            var minFitness = double.MaxValue;

            var currency = request.RunRequest.Balance;
            var budgetExtra = 0d;
            var extendedResults = results.Select(Result =>
            {
                var cost = Result.Sz * Result.Pr;
                currency -= cost;
                if (currency > request.RunRequest.Balance + budgetExtra)
                {
                    budgetExtra = currency - request.RunRequest.Balance;
                }
                return new { Result, BudgetExtra = budgetExtra };
            }).ToList();

            for (var i = 0; i < frames; i++)
            {
                var f0 = (gk * i) + firstTime;
                var f1 = (gk * (i + 1)) + firstTime;
                var frameTrades = extendedResults
                    .SkipWhile(x => x.Result.Tm < f0)
                    .TakeWhile(x => x.Result.Tm < f1)
                    .ToList();

                var currentBudgetExtra = frameTrades.LastOrDefault()?.BudgetExtra ?? lastBudgetExtra;
                var tradeFactor = 1;
                var fitness = tradeFactor * (currentBudgetExtra - lastBudgetExtra);
                if (fitness < minFitness)
                {
                    minFitness = fitness;
                }
                lastBudgetExtra = currentBudgetExtra;
            }
            return minFitness;
        }

        private static double IncomePerDayRatio(
            ICollection<RunResponse> results
            )
        {
            if (results.Count < 2)
            {
                return 0;
            }

            var firstResult = results.First();
            var lastResult = results.Last();

            var totalDays = (lastResult.Tm - firstResult.Tm) / 86400000;

            if (totalDays <= 0)
            {
                return 0;
            }

            var backtestStartingPoint = firstResult.Tm;

            double goodDay = 0;

            for (var day = 0; day < totalDays; day++)
            {
                var firstChunkTrade = backtestStartingPoint + day * 86400000;
                var lastChunkTrade = backtestStartingPoint + (day + 1) * 86400000;

                var dayTrades = results
                    .Where(x => x.Tm >= firstChunkTrade && x.Tm < lastChunkTrade)
                    .ToList();

                if (!dayTrades.Any()) continue;

                var np = dayTrades.Last().Np - dayTrades.First().Np;
                var pl = dayTrades.Last().Pl - dayTrades.First().Pl;

                if (pl > 0 && np > 0)
                {
                    goodDay += 1;
                    double incomeThatDay = PercentageDifference(dayTrades.First().Np, dayTrades.Last().Np);
                    if (incomeThatDay > 1)
                    {
                        goodDay += 1;
                    }
                }
            }

            return (double)goodDay / totalDays;
        }

        public static FitnessComposition NpaRrr(BacktestRequest request, ICollection<RunResponse> results)
        {
            if (results == null || results.Count == 0) return new FitnessComposition();

            var result = new FitnessComposition
            {
                Fitness = (IncomePerDayRatio(results))
            };

            return result;
        }

        public static double PercentageDifference(
            double firstValue,
            double secondValue
        )
        {
            double numerator = Math.Abs(firstValue - secondValue);
            double denominator = (firstValue + secondValue) / 2;

            if (numerator != 0)
            {
                double percentageDiff = (numerator / denominator) * 100;
                return percentageDiff;
            }

            return 0;
        }

    }
}