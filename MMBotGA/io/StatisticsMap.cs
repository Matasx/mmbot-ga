using CsvHelper.Configuration;
using MMBotGA.backtest;

namespace MMBotGA.io;

internal class StatisticsMap : ClassMap<Statistics>
{
    public StatisticsMap()
    {
        Map(x => x.TradeCount).Index(20);
    }
}