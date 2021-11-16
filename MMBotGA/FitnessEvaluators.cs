using MMBotGA.dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MMBotGA
{
    internal static class FitnessEvaluators
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

            var result = max_pl / max_downdraw;
            return Math.Max(result, 0);
        }

        public static double TradeCountFactor(ICollection<RunResponse> results)
        {
            // Avoid small trade count, that leads to overfitting
            const double minTarget = 500d;
            return Math.Min(minTarget, results.Count) / minTarget;
        }

        // todo: fitness that considers continual income over time

        public static double NormalizedProfitPerYear(BacktestRequest request, ICollection<RunResponse> results)
        {
            // npc: https://github.com/ondra-novak/mmbot/blob/141f74206f7b1938fa0903d20486f4962293ad1e/www/admin/code.js#L1872

            if (results.Count < 2) return 0;
            var last = results.Last();
            var first = results.First();

            var interval = last.Tm - first.Tm;
            return Math.Max(last.Npl * 31536000000 / (interval * request.RunRequest.Balance), 0);
        }

        public static double NpaRRR(BacktestRequest request, ICollection<RunResponse> results)
        {
            return NormalizedProfitPerYear(request, results) * RRR(results) * TradeCountFactor(results);
        }
    }
}
