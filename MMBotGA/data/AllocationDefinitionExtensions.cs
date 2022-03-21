﻿using MMBotGA.ga;

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
                AdamChromosome = allocationDefinition.AdamChromosome ?? new GammaChromosome() //Pokud neni zvoleny AdamChromosome, default je EpaChromosome()
            };
        }
    }
}