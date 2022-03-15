using CsvHelper.Configuration;
using MMBotGA.backtest;

namespace MMBotGA.io
{
    internal class StatisticsMap : ClassMap<Statistics>
    {
        public StatisticsMap()
        {
            Map(x => x.TradeCount).Index(100);
            Map(x => x.Balance).Index(101);

            Map(x => x.PlAbs).Index(102);
            Map(x => x.PlYrAbs).Index(103);
            Map(x => x.PlPerc).Index(104);

            Map(x => x.NormPlAbs).Index(105);
            Map(x => x.NormPlYrAbs).Index(106);
            Map(x => x.NormPlPerc).Index(107);

            Map(x => x.MaxPosAbs).Index(108);
            Map(x => x.MaxCostAbs).Index(109);
            Map(x => x.MaxProfitAbs).Index(110);
            Map(x => x.MaxLossAbs).Index(111);
            Map(x => x.MaxLossPerc).Index(112);

            Map(x => x.RRR).Index(113);
        }
    }
}