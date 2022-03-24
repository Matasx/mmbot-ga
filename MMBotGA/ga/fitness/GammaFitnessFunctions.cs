using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using MMBotGA.backtest;
using MMBotGA.dto;

namespace MMBotGA.ga.fitness
{
    internal static class GammaFitnessFunctions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GammaFitnessFunctions));

        public static double CheckForEvents(
            ICollection<RunResponse> results
        )
        {
            if (results.Where(x => x.Event != null).Where(x => x.Event == "margin_call").Count() > 0)
            {
                return 0;
            }
            else
            {
                return 1;
            };
        }

        public static double TightenNplRpnlSubmergedFunction(
            ICollection<RunResponse> results,
            double tightenNplRpnlThreshold,
            double tightenEquityThreshold,
            double howDeepToDive
        )
        {
            if (results.Count == 0) { return 0; }

            double deviatedTrades = 0;
            double resultsCounted = results.Count();

            int index = 0;

            foreach (var result in results)
            {
                double npl = result.Npl;
                double rpnl = result.Rpnl;
                double tradeSize = result.Sz;
                double percentageDiffCalculation = PercentageDifference(npl, rpnl);
                if (tradeSize != 0)
                {
                    if (percentageDiffCalculation > tightenNplRpnlThreshold)
                    {

                        //Lze nahradit IPDR.
                        
                        //if ((index) > 0)
                        //{
                        //    RunResponse whatHappenedBefore = results.ElementAt(index - 1);
                        //    if (whatHappenedBefore.Pr != 0)
                        //    {
                        //        double prBefore = whatHappenedBefore.Pr;
                        //        double prActual = result.Pr;
                        //        double percentageDiffPriceCalculation = PercentageDifference(prBefore, prActual);
                        //        if (percentageDiffPriceCalculation > 3.5) //Cenový rozdíl 3,5 procenta (dump/pump).
                        //        {
                        //            //Nefunguje z nějakého důvodu, přestane obchodovat.
                        //            //deviatedTrades += -1;
                        //        }
                        //    }
                        //}

                        deviatedTrades += 1;

                        if (GetEquityToFollow(result, tightenEquityThreshold))
                        {
                            deviatedTrades += 1;
                        }
                    }

                    if (null != result.Info)
                    {
                        double budgetCurrent = result.Info.BudgetCurrent;
                        double budgetMax = result.Info.BudgetMax;
                        double percentageDiffBudgetCalc = PercentageDifference(budgetCurrent, budgetMax);

                        if (percentageDiffBudgetCalc > howDeepToDive) { deviatedTrades += 1; }
                    }
                }
                if (tradeSize == 0) { deviatedTrades += 1.5; }
                index++;
            }

            //slouží jako ratio. deviatedTrades může mít za jeden trade skóre až 3, přičemž se mu snižuje celková fitness na základě odpočtu
            //od celkového počtu obchodů.
            double deviatedTradesRatio = deviatedTrades / resultsCounted;
            double deviationThresholdActual = 1 - deviatedTradesRatio;
            return deviationThresholdActual;
        }

        public static bool GetEquityToFollow(
            RunResponse result,
            double tightenEquityFollow
        )
        {
            bool deviated = false;
            double pl = result.Pl;
            double rpnl = result.Rpnl;

            double percentageDiffCalculation = PercentageDifference(pl, rpnl);
            if (percentageDiffCalculation > tightenEquityFollow) { deviated = true; }

            return deviated;
        }

        public static double Rrr(
            ICollection<RunResponse> results
        )
        {
            if (results.Count < 1) return 0;

            double maxPl = 0, minPl = 0, maxDowndraw = 0;
            foreach (var trade in results)
            {
                if (trade.Pl > maxPl)
                {
                    minPl = maxPl = trade.Pl;
                }

                if (trade.Pl < minPl)
                {
                    minPl = trade.Pl;
                    var downdraw = maxPl - minPl;
                    if (downdraw > maxDowndraw)
                    {
                        maxDowndraw = downdraw;
                    }
                }
            }

            var result = Math.Max(maxPl / maxDowndraw, 0);
            return Normalize(result, 5, 30, null);
        }

        private static double PnlProfitPerYear(
            BacktestRequest request,
            ICollection<RunResponse> results
        )
        {
            if (results.Count < 2) return 0;
            var last = results.Last();
            var first = results.First();

            var interval = last.Tm - first.Tm;
            var profit = Math.Max(last.Pl * 31536000000 / (interval * request.RunRequest.Balance), 0);

            return profit;
        }

        public static FitnessComposition NpaRrr(
            BacktestRequest request,
            ICollection<RunResponse> results
        )
        {
            if (results == null || results.Count == 0) return new FitnessComposition();


            const double rrrWeight = 0.20;
            const double tightenNplRpnlWeight = 0.80;

            const double tightenNplRpnlThreshold = 1.5; // % oscilace profit&loss kolem normalized profit.
            const double tightenEquityThreshold = 3;
            const double howDeepToDive = 20;

            var eventCheck = CheckForEvents(results); //0-1, nic jiného nevrací.
            var result = new FitnessComposition();

            result.RRR = rrrWeight * Rrr(results);
            result.TightenNplRpnl = tightenNplRpnlWeight * TightenNplRpnlSubmergedFunction(results, tightenEquityThreshold, tightenNplRpnlThreshold, howDeepToDive);
            result.PnlProfitPerYear = PnlProfitPerYear(request, results);

            //it is a MUST for this fitness to be mathematically tied down by execution logic and budget handling by Gauss/HalfHalf under Gamma. Otherwise it will explode into extreme bets, using exponencial function.
            result.Fitness = result.PnlProfitPerYear * (result.RRR + result.TightenNplRpnl);

            return result;
        }



        public static double Normalize(
            double value,
            double target,
            double virtualMax,
            double? cap
        )
        {
            if (value <= 0) return 0;
            var capped = Math.Min(value, cap ?? value);
            var baseline = Math.Min(capped, target) / target;
            var aboveTarget = Math.Max(0, value - target);
            var vMaxAboveTarget = virtualMax - target;
            var extra = Math.Min(aboveTarget, vMaxAboveTarget) / vMaxAboveTarget;

            return 0.75 * baseline + 0.25 * extra;
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