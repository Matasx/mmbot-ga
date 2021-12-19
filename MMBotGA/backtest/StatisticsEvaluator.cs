using System.Collections.Generic;
using MMBotGA.dto;

namespace MMBotGA.backtest;

internal static class StatisticsEvaluator
{
    public static Statistics Evaluate(ICollection<RunResponse> data)
    {
        return new Statistics
        {
            TradeCount = data.Count
        };
    }
}