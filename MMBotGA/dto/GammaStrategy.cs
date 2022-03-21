using System.Text.Json.Serialization;

namespace MMBotGA.dto
{
    public class GammaStrategy : IStrategy
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("function")]
        public string Function { get; set; }

        [JsonPropertyName("exponent")]
        public double Exponent { get; set; }

        [JsonPropertyName("rebalance")]
        public string Rebalance { get; set; }

        [JsonPropertyName("trend")]
        public double Trend { get; set; }

        [JsonPropertyName("reinvest")]
        public bool Reinvest { get; set; }
    }
}
