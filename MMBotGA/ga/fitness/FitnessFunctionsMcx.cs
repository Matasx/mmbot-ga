﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using log4net;
using MMBotGA.backtest;
using MMBotGA.dto;

namespace MMBotGA.ga.fitness
{
    internal static class FitnessFunctionsMcx
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FitnessFunctionsMcx));

        private static double LowerPositionOverall(BacktestRequest request, ICollection<RunResponse> results, double balancePercentage)
        {
            if (results.Count < 1) return 0;

            var balanceEval = balancePercentage * request.RunRequest.Balance;

            //All trades with position above x% of balance
            var tradesHighPosition = results.Count(x => x.Pr * x.Ps > balanceEval);

            var lowPosOverall = 1 - (double)tradesHighPosition / results.Count;
            return lowPosOverall;
        }

        private static double MaxCost(BacktestRequest request, ICollection<RunResponse> results, double maxCostThreshold)
        {
            double cost = 0;
            double maxCost = 0;

            foreach (var trade in results)
            {
                cost += trade.Sz * trade.Pr;
                if (cost > maxCost) { maxCost = cost; }
            }

            var balance = request.RunRequest.Balance;
            var budgetRatio = maxCost / balance;
            var budgetRatioInverse = 1 - maxCost / balance;
            if (budgetRatio < maxCostThreshold) return 1;

            return budgetRatioInverse;
        }

        private static double Rrr(ICollection<RunResponse> results)
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

        private static double TradeCountFactor(ICollection<RunResponse> results)
        {
            if (results.Count < 2) return 0;
            var last = results.Last();
            var first = results.First();

            var trades = results.Count(x => x.Sz != 0);
            var alerts = 1 - (results.Count - trades) / (double) results.Count;

            if (trades == 0 || alerts / trades > 0.03) return 0;

            var days = (last.Tm - first.Tm) / 86400000d;
            var tradesPerDay = trades / days;

            const int mean = 6;
            const int delta = 3; // target trade range is 5 - 15 trades per day

            var x = Math.Abs(tradesPerDay - mean); // 0 - inf, 0 is best
            var y = Math.Max(x - delta, 0) + 1; // 1 - inf, 1 is best ... 
            var r = 1 / y;

            return r * alerts;

            //return Normalize(trades, 1000, 3000, null) * alerts;
        }

        private static double IncomePerDayRatio(ICollection<RunResponse> results)
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

        private static double RpnlFactor(BacktestRequest request, ICollection<RunResponse> results)
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

            var sum = 0d;
            for (var day = 0; day < totalDays; day++)
            {
                var firstChunkTrade = backtestStartingPoint + day * 86400000;
                var lastChunkTrade = backtestStartingPoint + (day + 1) * 86400000;

                var dayTrades = results
                    .Where(x => x.Tm >= firstChunkTrade && x.Tm < lastChunkTrade)
                    .ToList();

                if (!dayTrades.Any()) continue;

                //var np = dayTrades.Last().Np - dayTrades.First().Np;
                var pl = dayTrades.Last().Pl - dayTrades.First().Pl;
                var perc = pl / request.RunRequest.Balance;
                double norm;
                
                // daily profit above 0.3% (e.g. 30$ of 10k budget)
                if (perc < 0)
                    norm = -Normalize(-perc, 0.003, 0.01, null);
                else
                    norm = Normalize(perc, 0.003, 0.01, null);

                sum += norm;
            }

            return Math.Max(sum / totalDays, 0);
        }

        private static double NormalizedProfitPerYear(BacktestRequest request, ICollection<RunResponse> results)
        {
            // npc: https://github.com/ondra-novak/mmbot/blob/141f74206f7b1938fa0903d20486f4962293ad1e/www/admin/code.js#L1872

            if (results.Count < 2) return 0;
            var last = results.Last();
            var first = results.First();

            var interval = last.Tm - first.Tm;
            var profit = Math.Max(last.Npl * 31536000000 / (interval * request.RunRequest.Balance), 0);

            if (profit == 0) return 0;

            return Normalize(profit, 1, 3, null);
        }

        private static double PnlProfitPerYear(BacktestRequest request, ICollection<RunResponse> results)
        {
            // pc: https://github.com/ondra-novak/mmbot/blob/141f74206f7b1938fa0903d20486f4962293ad1e/www/admin/code.js#L1873

            if (results.Count < 2) return 0;
            var last = results.Last();
            var first = results.First();

            var interval = last.Tm - first.Tm;
            var profit = Math.Max(last.Pl * 31536000000 / (interval * request.RunRequest.Balance), 0);

            if (profit == 0) return 0;

            return Normalize(profit, 1, 3, null);
        }

        public static FitnessComposition NpaRrr(BacktestRequest request, ICollection<RunResponse> results)
        {
            if (results == null || results.Count == 0) return new FitnessComposition();

            const double pppyWeight = 0.15;
            const double ipdrWeight = 0.05;
            const double rrrWeight = 0.05;
            const double tradeCountWeight = 0.3;
            const double lpoWeight = 0.10;
            const double mcWeight = 0.15;
            const double rpnlWeight = 0.20;

            Debug.Assert(Math.Abs(pppyWeight + ipdrWeight + lpoWeight + rrrWeight + tradeCountWeight + mcWeight + rpnlWeight - 1) < 0.01);

            const double balanceThreshold = 0.1;
            const double maxCostThreshold = 0.6;

            var result = new FitnessComposition();
            result.Fitness = pppyWeight * (result.PnlProfitPerYear = PnlProfitPerYear(request, results))
                          + ipdrWeight * (result.IncomePerDayRatio = IncomePerDayRatio(results))
                          + rrrWeight * (result.RRR = Rrr(results))
                          + tradeCountWeight * (result.TradeCountFactor = TradeCountFactor(results))
                          + lpoWeight * (result.LowerPositionFactor = LowerPositionOverall(request, results, balanceThreshold))
                          + mcWeight * (result.MaxCostFactor = MaxCost(request, results, maxCostThreshold))
                          + rpnlWeight * (result.RpnlFactor = RpnlFactor(request, results));

            Log.Debug($"Fitness : {result.Fitness}");

            return result;
        }

        private static double Normalize(double value, double target, double virtualMax, double? cap)
        {
            if (value <= 0) return 0;
            var capped = Math.Min(value, cap ?? value);
            var baseline = Math.Min(capped, target) / target;
            var aboveTarget = Math.Max(0, value - target);
            var vMaxAboveTarget = virtualMax - target;
            var extra = Math.Min(aboveTarget, vMaxAboveTarget) / vMaxAboveTarget;

            return 0.75 * baseline + 0.25 * extra;
        }
    }
}