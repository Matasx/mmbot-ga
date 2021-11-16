using CsvHelper.Configuration;

namespace MMBotGA
{
    internal class CsvMap : ClassMap<StrategyChromosome>
    {
        public CsvMap()
        {
            Map(x => x.Exponent).Index(0);
            Map(x => x.Trend).Index(1);
            Map(x => x.Rebalance).Index(2);
            Map(x => x.Stdev).Index(3);
            Map(x => x.Sma).Index(4);
            Map(x => x.Mult).Index(5);
            Map(x => x.Raise).Index(6);
            Map(x => x.Fall).Index(7);
            Map(x => x.Cap).Index(8);
            Map(x => x.Mode).Index(9);
            Map(x => x.Fitness).Index(10);
            Map(x => x.Metadata).Index(11);
        }
    }
}
