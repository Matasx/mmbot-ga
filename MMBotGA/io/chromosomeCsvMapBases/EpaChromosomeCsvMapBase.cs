using MMBotGA.ga;

namespace MMBotGA.io
{
    internal class EpaChromosomeCsvMapBase : SpreadChromosomeCsvMapBase<EpaChromosome>
    {
        public EpaChromosomeCsvMapBase(bool aggregated) : base(aggregated)
        {
            Map(x => x.MinAssetPercOfBudget).Index(3);
            Map(x => x.InitialBetPercOfBudget).Index(4);
            Map(x => x.MaxEnterPriceDistance).Index(5);
            Map(x => x.PowerMult).Index(6);
            Map(x => x.PowerCap).Index(7);
            Map(x => x.Angle).Index(8);
            Map(x => x.TargetExitPriceDistance).Index(9);
            Map(x => x.ExitPowerMult).Index(10);
        }
    }
}
