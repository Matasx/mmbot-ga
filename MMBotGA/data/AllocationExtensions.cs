namespace MMBotGA.data
{
    internal static class AllocationExtensions
    {
        public static string ToBatchName(this Allocation allocation)
        {
            return $"{allocation.Symbol}@{allocation.Exchange}";
        }
    }
}