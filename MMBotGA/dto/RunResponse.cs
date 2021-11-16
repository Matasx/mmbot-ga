using System.Text.Json.Serialization;

namespace MMBotGA.dto
{
    public class RunResponse
    {
        [JsonPropertyName("bal")]
        public double Bal { get; set; }

        [JsonPropertyName("na")]
        public int Na { get; set; }

        [JsonPropertyName("np")]
        public double Np { get; set; }

        [JsonPropertyName("npl")]
        public double Npl { get; set; }

        [JsonPropertyName("npla")]
        public double Npla { get; set; }

        [JsonPropertyName("op")]
        public int Op { get; set; }

        [JsonPropertyName("pl")]
        public double Pl { get; set; }

        [JsonPropertyName("pr")]
        public double Pr { get; set; }

        [JsonPropertyName("ps")]
        public double Ps { get; set; }

        [JsonPropertyName("sz")]
        public double Sz { get; set; }

        [JsonPropertyName("tm")]
        public long Tm { get; set; }

        [JsonPropertyName("ubal")]
        public double Ubal { get; set; }

        [JsonPropertyName("info")]
        public Info Info { get; set; }
    }
}
