using System;
using System.Collections.Generic;
using System.Linq;
using MMBotGA.dto;

namespace MMBotGA.backtest
{
    internal static class StatisticsEvaluator
    {
        public static Statistics Evaluate(BacktestRequest request, ICollection<RunResponse> data)
        {
            if (data.Count == 0) return new Statistics();

            double max_pl = 0, min_pl = 0, max_downdraw = 0, cost = 0, max_cost = 0, max_pos = 0;
            var vlast = data.Last();
            var interval = Math.Max(vlast.Tm - data.First().Tm, 0.0000001);

            foreach (var trade in data)
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

                cost += trade.Sz * trade.Pr;
                if (cost > max_cost) max_cost = cost;

                var ap = Math.Abs(trade.Ps);
                if (ap > max_pos) max_pos = ap;
            }

            return new Statistics
            {
                TradeCount = data.Count,
                Balance = request.RunRequest.Balance,

                PlAbs = vlast.Pl,
                PlYrAbs = vlast.Pl * 31536000000 / interval,
                PlPerc = vlast.Pl * 31536000000 / (interval * request.RunRequest.Balance),

                NormPlAbs = vlast.Npl,
                NormPlYrAbs = vlast.Npl * 31536000000 / interval,
                NormPlPerc = vlast.Npl * 31536000000 / (interval * request.RunRequest.Balance),

                MaxPosAbs = max_pos,
                MaxCostAbs = max_cost,
                MaxProfitAbs = max_pl,
                MaxLossAbs = -max_downdraw,
                MaxLossPerc = max_downdraw / request.RunRequest.Balance * 100,
                RRR = Math.Max(max_pl / max_downdraw, 0)
            };
        }
    }
}