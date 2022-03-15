using CsvHelper.Configuration;
using MMBotGA.ga.fitness;

namespace MMBotGA.io
{
    internal class FitnessCompositionMap : ClassMap<FitnessComposition>
    {
        public FitnessCompositionMap()
        {
            Map(x => x.IncomePerDayRatio).Index(200);
            Map(x => x.LowerPositionFactor).Index(201);
            Map(x => x.MaxCostFactor).Index(202);
            Map(x => x.PnlProfitPerYear).Index(203);
            Map(x => x.RRR).Index(204);
            Map(x => x.RpnlFactor).Index(205);
            Map(x => x.TradeCountFactor).Index(206);
            Map(x => x.NpProfitPerYear).Index(207);
            Map(x => x.MinMaxBalanceTheBalanceFactor).Index(208);
            Map(x => x.TightenNplRpnl).Index(209);
        }
    }
}