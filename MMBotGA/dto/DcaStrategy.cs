using System.Text.Json.Serialization;

namespace MMBotGA.dto
{
    public class DcaStrategy : IStrategy
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("r")]
        public double Reduction { get; set; }

        [JsonPropertyName("w")]
        public double Power { get; set; }

        [JsonPropertyName("z")]
        public double Exponent { get; set; }

        [JsonPropertyName("ms")]
        public double MaxSpread { get; set; }

        [JsonPropertyName("ri")]
        public bool ReinvestProfit { get; set; }

        [JsonPropertyName("invert_proxy")]
        public bool InvertProxy { get; set; }
    }
}
