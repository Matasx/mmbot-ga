using CsvHelper.Configuration;

namespace MMBotGA
{
    internal class CsvMap : ClassMap<StrategyChromosome>
    {
        public CsvMap()
        {
            Map(x => x.ID).Index(0);
            Map(x => x.Generation).Index(1);
            Map(x => x.Exponent).Index(2);
            Map(x => x.Trend).Index(3);
            Map(x => x.Rebalance).Index(4);
            Map(x => x.Stdev).Index(5);
            Map(x => x.Sma).Index(6);
            Map(x => x.Mult).Index(7);
            Map(x => x.Raise).Index(8);
            Map(x => x.Fall).Index(9);
            Map(x => x.Cap).Index(10);
            Map(x => x.Mode).Index(11);
            Map(x => x.Fitness).Index(12);
            Map(x => x.Metadata).Index(100);

            References<StatisticsMap>(x => x.Statistics).Prefix("Stat.");
        }
    }
}
