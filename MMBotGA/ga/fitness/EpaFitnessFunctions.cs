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
                var tradeFactor = 1; // TradeCountFactor(frameTrades);
                var fitness = tradeFactor * (currentBudgetExtra - lastBudgetExtra);
                if (fitness < minFitness)
                {
                    minFitness = fitness;
                }
                lastBudgetExtra = currentBudgetExtra;
            }
            return minFitness;
        }

        public static FitnessComposition NpaRrr(BacktestRequest request, ICollection<RunResponse> results)
        {
            if (results == null || results.Count == 0) return new FitnessComposition();

            var result = new FitnessComposition
            {
                Fitness = (EvaluateFit(request, results))
            };

            #region Outdated
            //result.Fitness = (nppyWeight * (result.NpProfitPerYear = NormalizedProfitPerYear(request, results))
            //  + pppyWeight * (result.PnlProfitPerYear = PnlProfitPerYear(request, results))
            //  + ipdrWeight * (result.IncomePerDayRatio = IncomePerDayRatio(results))
            //  + rrrWeight * (result.RRR = Rrr(results))
            //  + tradeCountWeight * (result.TradeCountFactor = TradeCountFactor(results))
            //  + lpoWeight * (result.LowerPositionFactor = LowerPositionOverall(request, results, balanceThreshold))
            //  + maxCostWeight * (result.MaxCostFactor = MaxCost(request, results))
            //  + tightenNeutralPriceWeight * (result.TightenNeutralPriceToLast = TightenNeutralPriceToLast(results, tightenNeutralPriceThreshold)))
            //  + tightenNplRpnlWeight * (result.TightenNplRpnl = TightenNplRpnl(results, tightenNplRpnlThreshold))
            //  * eventCheck;
            //var fitness = (nppyEval + pppyEval + ipdrEval + rrrEval + tradeCountEval + lowerPosEval + maxCostEval + minMaxBalanceTheBalanceEval) * eventCheck;
            //Formát výpisu zachovat, čárka a mezera se používají v LogAnalyzer.ps1 dle které se splitují hodnoty !
            //Log.Info($"Fitness : {fitness}, nppyEval : {nppyEval}, pppyEval : {pppyEval}, ipdrEval : {ipdrEval}, rrrEval : {rrrEval}, tradeCountEval : {tradeCountEval}, lowerPosEval : {lowerPosEval}, MaxCostEval : {maxCostEval}, EventCheck : {eventCheck}");
            #endregion

            return result;
        }
    }
}