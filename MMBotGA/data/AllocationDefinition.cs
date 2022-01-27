using MMBotGA.data.exchange;
using MMBotGA.ga;

namespace MMBotGA.data
{
    internal class AllocationDefinition
    {
        public IExchange Exchange { get; set; }
        public Pair Pair { get; set; }
        public double Balance { get; set; }
        public ICustomChromosome AdamChromosome { get; set; }
    }
}