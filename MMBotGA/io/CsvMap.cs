using CsvHelper.Configuration;
using MMBotGA.ga;

namespace MMBotGA.io
{
    internal class CsvMap : ClassMap<StrategyChromosome>
    {
        public CsvMap()
        {
            Map(x => x.ID).Index(0);
            Map(x => x.Generation).Index(1);
            Map(x => x.Fitness).Index(2);
            Map(x => x.Function).Index(3);
            Map(x => x.Exponent).Index(4);
            Map(x => x.Trend).Index(5);
            Map(x => x.Rebalance).Index(6);
            Map(x => x.Stdev).Index(7);
            Map(x => x.Sma).Index(8);
            Map(x => x.Mult).Index(9);
            Map(x => x.Raise).Index(10);
            Map(x => x.Fall).Index(11);
            Map(x => x.Cap).Index(12);
            Map(x => x.Mode).Index(13);
            Map(x => x.Freeze).Index(14);
            Map(x => x.DynMult).Index(15);
            Map(x => x.Metadata).Index(100);

            References<StatisticsMap>(x => x.BacktestStats).Prefix("BT_");
            References<StatisticsMap>(x => x.ControlStats).Prefix("CT_");
        }
    }
}
