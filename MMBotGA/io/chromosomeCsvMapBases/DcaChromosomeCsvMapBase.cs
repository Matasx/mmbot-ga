using MMBotGA.ga;

namespace MMBotGA.io
{
    internal class DcaChromosomeCsvMapBase : SpreadChromosomeCsvMapBase<DcaChromosome>
    {
        public DcaChromosomeCsvMapBase(bool aggregated) : base(aggregated)
        {
            Map(x => x.Power).Index(3);
            Map(x => x.Exponent).Index(4);
            Map(x => x.Reduction).Index(5);
            Map(x => x.MaxSpread).Index(6);
        }
    }
}