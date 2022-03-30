﻿using MMBotGA.ga;

namespace MMBotGA.io
{
    internal class GammaChromosomeCsvMapBase : SpreadChromosomeCsvMapBase<GammaChromosome>
    {
        public GammaChromosomeCsvMapBase(bool aggregated) : base(aggregated)
        {
            Map(x => x.Function).Index(3);
            Map(x => x.Exponent).Index(4);
            Map(x => x.Trend).Index(5);
            //Map(x => x.Rebalance).Index(6);
        }
    }
}
