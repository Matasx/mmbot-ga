using MMBotGA.ga;
using System.Text.Json.Serialization;

namespace MMBotGA.data
{
    internal class Allocation
    {
        public string Exchange { get; set; }
        public string Symbol { get; set; }
        public string RobotSymbol { get; set; }
        public double Balance { get; set; }
        [JsonIgnore]
        public ICustomChromosome AdamChromosome { get; set; }
    }
}