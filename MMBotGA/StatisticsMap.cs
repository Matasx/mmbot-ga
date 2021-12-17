using CsvHelper.Configuration;

namespace MMBotGA;

internal class StatisticsMap : ClassMap<Statistics>
{
    public StatisticsMap()
    {
        Map(x => x.TradeCount).Index(20);
    }
}