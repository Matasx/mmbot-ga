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
            double tightenEquityThreshold
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

                        //if (percentageDiffBudgetCalc > howDeepToDive) { deviatedTrades += 1; }
                    }
                }
                if (tradeSize == 0) { 
                    deviatedTrades += 1.5;
                }
                index++;
            }

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

            var goodDay = 0;
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
                    goodDay++;
                }
            }

            return (double)goodDay / totalDays;
        }



        private static double ensureMinimumTradeCount(
            ICollection<RunResponse> results,
            int tradesPerDayThreshold
        )
        {
            if (results.Count < 2) return 0;
            var last = results.Last();
            var first = results.First();

            var trades = results.Count(x => x.Sz != 0);
            var alerts = 1 - (results.Count - trades) / (double)results.Count;

            if (trades == 0 || alerts / trades > 0.02) return 0; //alerts / trades > 0.02

            var days = (last.Tm - first.Tm) / 86400000d;
            var tradesPerDay = trades / days;

            if (tradesPerDay > tradesPerDayThreshold) { return 1; } else { return 0; }
        }

        public static FitnessComposition NpaRrr(
            BacktestRequest request,
            ICollection<RunResponse> results
        )
        {
            if (results == null || results.Count == 0) return new FitnessComposition();


            const double rrrWeight = 0.1;
            const double tightenNplRpnlWeight = 0.3;
            const double ipdrWeight = 0.6;

            const double tightenNplRpnlThreshold = 2; // % oscilace profit&loss kolem normalized profit.
            const double tightenEquityThreshold = 2;

            var eventCheck = CheckForEvents(results); //0-1, nic jiného nevrací.
            var result = new FitnessComposition();

            result.RRR = rrrWeight * Rrr(results);
            result.TightenNplRpnl = tightenNplRpnlWeight * TightenNplRpnlSubmergedFunction(results, tightenEquityThreshold, tightenNplRpnlThreshold);
            result.IncomePerDayRatio = ipdrWeight * IncomePerDayRatio(results);            

            var fitnessReach = result.RRR + result.TightenNplRpnl + result.IncomePerDayRatio;
            //var ppyCalc = Math.Pow((PnlProfitPerYear(request, results) * 100), ppyExponent);

            //if (Double.IsInfinity(ppyCalc)) { result.PnlProfitPerYear = 0; }
            //else { result.PnlProfitPerYear = ppyCalc; }

            #region NextPhaseOfCalculation
            if (fitnessReach > 0.45) //Heavily depends on tradeable pair.
            {
                //Solve Income
                var pnlProfitPerYear = PnlProfitPerYear(request, results);
                if (Double.IsInfinity(pnlProfitPerYear)) 
                { result.Fitness = fitnessReach; } 
                else 
                { result.Fitness = fitnessReach * pnlProfitPerYear; }

            } else
            {
                result.Fitness = fitnessReach;
            }
            #endregion

            //It is a MUST for this fitness to be mathematically tied down by execution logic and budget handling by Gauss/HalfHalf under Gamma.
            //Otherwise it will explode into extreme bets, using exponencial function.
            //Also this fitness neeeds a kickstart in form of a ensureMinimumTradeCount(results, minimum trades per day), which multiplies the whole result (e.g. if under minTradesPerDay, whole results is 0). 
            //result.Fitness = (result.PnlProfitPerYear * (result.TightenNplRpnl + result.RRR + result.IncomePerDayRatio)) * ensureMinimumTradeCount(results, 8);

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