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
                Balance = allocationDefinition.Balance
            };
        }
    }
}