using System;
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

        public static double BalanceTheBalance(ICollection<RunResponse> results, BacktestRequest request, double minBalance, double maxBalance)
        {
            var minBalanceThreshold = request.RunRequest.Balance * minBalance;
            var maxBalanceThreshold = request.RunRequest.Balance * maxBalance;

            //Pokud je pozice větší jak maxBalanceThreshold, zahazuj (vrať 0), pokud je pozice menší jak maxBalance a větší jak minBalance vrať jedna
            //Pokud je ale i zároveň menší jak minBalance zahazuj (vrať 0). 
            if (results.Where(x => x.Ps * x.Pr > maxBalanceThreshold).Count() > 1) { return 0; } else { return 1; };

            //if (results.Where(x => x.Ps * x.Pr > minBalanceThreshold).Count() > 1) { return 1; } else { return 0; };
        }
        public static double CheckForEvents(ICollection<RunResponse> results)
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

        public static double MaxCost(BacktestRequest request, ICollection<RunResponse> results, double maxCostThreshold)
        {
            double cost = 0;
            double maxCost = 0;

            foreach (var trade in results)
            {
                cost = cost + (trade.Sz * trade.Pr);
                if (cost > maxCost) { maxCost = cost; }
            }

            var balance = request.RunRequest.Balance;
            var budgetRatio = (maxCost / balance);
            var budgetRatioInverse = 1 - (maxCost / balance);

            //Čím menší MaxCost tím lepší Fitness.
            //Pokud jsi zainvestoval méně jak 50% dostáváš boost do budgetRatio. 
            //Myšlenka je motivovat vývoj k menšímu (většímu) budgetRatio, než-li penalizovat generaci.
            if (budgetRatio < maxCostThreshold) { return 1; }

            return budgetRatioInverse;
        }

        public static double LowerPositionOverall(BacktestRequest request, ICollection<RunResponse> results, double balancePercentage)
        {
            if (results.Count < 1) return 0;

            var balanceEval = balancePercentage * request.RunRequest.Balance;

            //All trades with position above x% of balance
            var tradesHighPosition = results.Count(x => x.Pr * x.Ps > balanceEval);

            var lowPosOverall = 1 - (double)tradesHighPosition / results.Count;
            return lowPosOverall;
        }

        public static double Rrr(ICollection<RunResponse> results)
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

        public static double TradeCountFactor(ICollection<RunResponse> results)
        {
            if (results.Count < 2) return 0;
            var last = results.Last();
            var first = results.First();

            var trades = results.Count(x => x.Sz != 0);
            var alerts = 1 - (results.Count - trades) / (double)results.Count;

            var days = (last.Tm - first.Tm) / 86400000d;
            var tradesPerDay = trades / days;

            const int mean = 15;
            const int delta = 7; // target trade range is 8 - 22 trades per day

            var x = Math.Abs(tradesPerDay - mean); // 0 - inf, 0 is best
            var y = Math.Max(x - delta, 0) + 1; // 1 - inf, 1 is best ... 
            var r = 1 / y;

            var alertsRatio = alerts / trades;

            //Pokud mám více jak 3% alertů v celém spektru, špatný backtest.
            if (alertsRatio > 0.03) { return 0; } else { return r * alerts; }



            //return Normalize(trades, 1000, 3000, null) * alerts;
        }

        public static double IncomePerDayRatio(ICollection<RunResponse> results)
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

                if (dayTrades.Any())
                {
                    var np = dayTrades.Last().Np - dayTrades.First().Np;
                    var pl = dayTrades.Last().Pl - dayTrades.First().Pl;

                    if (pl > 0 && np > 0)
                    {
                        goodDay++;
                    }
                }
            }

            return (double)goodDay / totalDays;
        }

        public static double NormalizedProfitPerYear(BacktestRequest request, ICollection<RunResponse> results)
        {
            // npc: https://github.com/ondra-novak/mmbot/blob/141f74206f7b1938fa0903d20486f4962293ad1e/www/admin/code.js#L1872

            if (results.Count < 2) return 0;
            var last = results.Last();
            var first = results.First();

            var interval = last.Tm - first.Tm;
            var profit = Math.Max(last.Npl * 31536000000 / (interval * request.RunRequest.Balance), 0);

            if (profit == 0) return 0;

            return Normalize(profit, 0.6, 1, null);
        }

        public static double PnlProfitPerYear(BacktestRequest request, ICollection<RunResponse> results)
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

        public static FitnessComposition NpaRRR(BacktestRequest request, ICollection<RunResponse> results)
        {
            if (results == null || results.Count == 0) return new FitnessComposition();

            const double nppyWeight = 0.05;
            const double pppyWeight = 0.05;
            const double ipdrWeight = 0.10;
            const double lpoWeight = 0.15;
            const double rrrWeight = 0.10;
            const double tradeCountWeight = 0.40;
            const double maxCostWeight = 0.10;
            const double minMaxBalanceTheBalanceWeight = 0.05;

            //Přes 40% 
            const double maxCostThreshold = 0.45;
            const double balanceThreshold = maxCostThreshold / 5;

            //Pozici nad 45% balance, zahazuj
            //Pozici pod 37% balance, zahazuj
            const double maxBalance = 0.50;
            const double minBalance = 0.37;

            //Debug
            Debug.Assert(Math.Abs(nppyWeight + pppyWeight + ipdrWeight + lpoWeight + rrrWeight + tradeCountWeight + maxCostWeight + minMaxBalanceTheBalanceWeight - 1) < 0.01);


            //var nppyEval = nppyWeight * NormalizedProfitPerYear(request, results);
            //var pppyEval = pppyWeight * PnlProfitPerYear(request, results);
            //var rrrEval = rrrWeight * Rrr(results);
            //var tradeCountEval = tradeCountWeight * TradeCountFactor(results);
            //var ipdrEval = ipdrWeight * IncomePerDayRatio(results);
            //var lowerPosEval = lpoWeight * LowerPositionOverall(request, results, balanceThreshold);
            //var maxCostEval = maxCostWeight * MaxCost(request, results, maxCostThreshold);
            //var minMaxBalanceTheBalanceEval = minMaxBalanceTheBalanceWeight * BalanceTheBalance(results, request, minBalance, maxBalance);
            var eventCheck = CheckForEvents(results);


            var result = new FitnessComposition();

            result.Fitness = (nppyWeight * (result.NpProfitPerYear = NormalizedProfitPerYear(request, results))
              + pppyWeight * (result.PnlProfitPerYear = PnlProfitPerYear(request, results))
              + ipdrWeight * (result.IncomePerDayRatio = IncomePerDayRatio(results))
              + rrrWeight * (result.RRR = Rrr(results))
              + tradeCountWeight * (result.TradeCountFactor = TradeCountFactor(results))
              + lpoWeight * (result.LowerPositionFactor = LowerPositionOverall(request, results, balanceThreshold))
              + maxCostWeight * (result.MaxCostFactor = MaxCost(request, results, maxCostThreshold))
              + minMaxBalanceTheBalanceWeight * (result.MinMaxBalanceTheBalanceFactor = BalanceTheBalance(results, request, minBalance, maxBalance)))
              * eventCheck;


            //var fitness = (nppyEval + pppyEval + ipdrEval + rrrEval + tradeCountEval + lowerPosEval + maxCostEval + minMaxBalanceTheBalanceEval) * eventCheck;

            //Formát výpisu zachovat, čárka a mezera se používají v LogAnalyzer.ps1 dle které se splitují hodnoty !
            //Log.Info($"Fitness : {fitness}, nppyEval : {nppyEval}, pppyEval : {pppyEval}, ipdrEval : {ipdrEval}, rrrEval : {rrrEval}, tradeCountEval : {tradeCountEval}, lowerPosEval : {lowerPosEval}, MaxCostEval : {maxCostEval}, EventCheck : {eventCheck}");

            return result;
        }

        public static double Normalize(double value, double target, double virtualMax, double? cap)
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
