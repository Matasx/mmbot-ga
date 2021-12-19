using System;
using System.Collections.Generic;
using System.Linq;
using MMBotGA.backtest;
using MMBotGA.dto;

namespace MMBotGA.ga.fitness
{
    internal static class FitnessFunctions
    {
        public static double RRR(ICollection<RunResponse> results)
        {
            double max_pl = 0, min_pl = 0, max_downdraw = 0;
            foreach (var trade in results)
            {
                if (trade.Pl > max_pl)
                {
                    min_pl = max_pl = trade.Pl;
                }
                if (trade.Pl < min_pl)
                {
                    min_pl = trade.Pl;
                    var downdraw = max_pl - min_pl;
                    if (downdraw > max_downdraw)
                    {
                        max_downdraw = downdraw;
                    }
                }
            }

            var result = Math.Max(max_pl / max_downdraw, 0);
            return Normalize(result, 5, 30, null);
        }

        public static double TradeCountFactor(ICollection<RunResponse> results)
        {
            if (results.Count < 0) return 0;
            var last = results.Last();
            var first = results.First();

            var trades = results.Where(x => x.Sz != 0).Count();
            var alerts = 1 - ((results.Count - trades) / (double)results.Count);

            var days = (last.Tm - first.Tm) / 86400000d;
            var tradesPerDay = trades / days;

            var mean = 10;
            var delta = 5; // target trade range is 5 - 15 trades per day

            var x = Math.Abs(tradesPerDay - mean); // 0 - inf, 0 is best
            var y = Math.Max(x - delta, 0) + 1; // 1 - inf, 1 is best ... 
            var r = 1 / y;

            return r * alerts;

            //return Normalize(trades, 1000, 3000, null) * alerts;
        }

        // todo: fitness that considers continual income over time

        public static double NormalizedProfitPerYear(BacktestRequest request, ICollection<RunResponse> results)
        {
            // npc: https://github.com/ondra-novak/mmbot/blob/141f74206f7b1938fa0903d20486f4962293ad1e/www/admin/code.js#L1872

            if (results.Count < 2) return 0;
            var last = results.Last();
            var first = results.First();

            var interval = last.Tm - first.Tm;
            var profit = Math.Max(last.Npl * 31536000000 / (interval * request.RunRequest.Balance), 0);
            return Normalize(profit, 0.6, 3, null);
        }

        public static double NpaRRR(BacktestRequest request, ICollection<RunResponse> results)
        {
            return (0.25 * NormalizedProfitPerYear(request, results)) + (0.25 * RRR(results)) + (0.5 * TradeCountFactor(results));
        }

        public static double Normalize(double value, double target, double virtualMax, double? cap)
        {
            if (value <= 0) return 0;
            var capped = Math.Min(value, cap ?? value);
            var baseline = Math.Min(capped, target) / target;
            var aboveTarget = Math.Max(0, value - target);
            var vMaxAboveTarget = virtualMax - target;
            var extra = Math.Min(aboveTarget, vMaxAboveTarget) / vMaxAboveTarget;

            return (0.75 * baseline) + (0.25 * extra);
        }
    }
}
