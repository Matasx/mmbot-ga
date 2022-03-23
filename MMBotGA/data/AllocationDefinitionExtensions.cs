using MMBotGA.ga;

namespace MMBotGA.data
{
    internal static class AllocationDefinitionExtensions
    {
        public static Allocation ToAllocation(this AllocationDefinition allocationDefinition)
        {
            return new Allocation
            {
                Exchange = allocationDefinition.Exchange.Name,
                Symbol = allocationDefinition.Exchange.GetSymbol(allocationDefinition.Pair),
                RobotSymbol = allocationDefinition.Exchange.GetRobotSymbol(allocationDefinition.Pair),
                Balance = allocationDefinition.Balance,
                //Default strategy to be used - GammaChromosome - Napsat selektor ? Selektor je AdamChromsome, Definovat defaultni hodnoty pro AdamChromosome?
                AdamChromosome = allocationDefinition.AdamChromosome ?? new GammaChromosome()
            };
        }
    }
}