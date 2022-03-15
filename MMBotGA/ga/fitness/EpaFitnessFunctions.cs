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
            // get profit at least every 14 days

            var timeFrame = results.Last().Tm - results.First().Tm; //timeFrame je delkagrafu * 60000L;
            var frames = (int)(TimeSpan.FromMilliseconds(timeFrame).TotalDays / 25);
            if (frames == 0) return 0;
            var gk = timeFrame / frames;
            var lastBudgetExtra = request.RunRequest.Balance; //results.First(x=>x.Info != null).Info.BudgetCurrency
            var minFitness = double.MaxValue;
            var incomeFrame = 0;
            //var fitnessSummed = 0d;

            for (var i = 0; i < frames; i++)
            {
                var f0 = (gk * i) + results.First().Tm;
                var f1 = (gk * (i + 1)) + results.First().Tm;
                var frameTrades = results
                    .SkipWhile(x => x.Tm < f0)
                    .TakeWhile(x => x.Tm < f1)
                    .ToList();

                //var profit = PnlProfitPerFrame(request, results);

                var currentBudgetExtra = frameTrades.LastOrDefault(x => x.Info != null)?.Bal ?? lastBudgetExtra;
                var tradeFactor = 1; // TradeCountFactor(frameTrades);
                var fitness = tradeFactor * (currentBudgetExtra - lastBudgetExtra);

                if (fitness > 0)
                {
                    incomeFrame++;
                }

                if (fitness < minFitness)
                {
                    minFitness = fitness;
                    //fitnessSummed = fitnessSummed + fitness;
                }
                lastBudgetExtra = currentBudgetExtra;
            }
            return incomeFrame;
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