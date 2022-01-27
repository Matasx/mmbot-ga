using MMBotGA.ga;

namespace MMBotGA.io
{
    internal class ChromosomeCsvMapBase : SpreadChromosomeCsvMapBase<StrategyChromosome>
    {
        public ChromosomeCsvMapBase(bool aggregated) : base(aggregated)
        {
            Map(x => x.Function).Index(3);
            Map(x => x.Exponent).Index(4);
            Map(x => x.Trend).Index(5);
            Map(x => x.Rebalance).Index(6);
        }
    }
}
