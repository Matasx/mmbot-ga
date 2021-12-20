using MMBotGA.data.exchange;

namespace MMBotGA.data
{
    internal class AllocationDefinition
    {
        public IExchange Exchange { get; set; }
        public Pair Pair { get; set; }
        public double Balance { get; set; }
    }
}