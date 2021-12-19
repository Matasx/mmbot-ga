using CsvHelper.Configuration;
using MMBotGA.backtest;

namespace MMBotGA.io;

internal class StatisticsMap : ClassMap<Statistics>
{
    public StatisticsMap()
    {
        Map(x => x.TradeCount).Index(20);
        Map(x => x.Balance).Index(21);

        Map(x => x.PlAbs).Index(22);
        Map(x => x.PlYrAbs).Index(23);
        Map(x => x.PlPerc).Index(24);

        Map(x => x.NormPlAbs).Index(25);
        Map(x => x.NormPlYrAbs).Index(26);
        Map(x => x.NormPlPerc).Index(27);

        Map(x => x.MaxPosAbs).Index(28);
        Map(x => x.MaxCostAbs).Index(29);
        Map(x => x.MaxProfitAbs).Index(30);
        Map(x => x.MaxLossAbs).Index(31);
        Map(x => x.MaxLossPerc).Index(32);

        Map(x => x.RRR).Index(33);
    }
}