using CsvHelper.Configuration;
using MMBotGA.ga.fitness;

namespace MMBotGA.io
{
    internal class FitnessCompositionMap : ClassMap<FitnessComposition>
    {
        public FitnessCompositionMap()
        {
            Map(x => x.IncomePerDayRatio).Index(40);
            Map(x => x.LowerPositionFactor).Index(41);
            Map(x => x.MaxCostFactor).Index(42);
            Map(x => x.PnlProfitPerYear).Index(43);
            Map(x => x.RRR).Index(44);
            Map(x => x.RpnlFactor).Index(45);
            Map(x => x.TradeCountFactor).Index(46);
            Map(x => x.NpProfitPerYear).Index(47);
            Map(x => x.MinMaxBalanceTheBalanceFactor).Index(48);
            Map(x => x.TightenNplRpnl).Index(49);
        }
    }
}