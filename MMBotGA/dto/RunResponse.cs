using System.Text.Json.Serialization;

namespace MMBotGA.dto
{
    public class RunResponse
    {
        [JsonPropertyName("bal")]
        public double Bal { get; set; }

        [JsonPropertyName("na")]
        public double Na { get; set; }

        [JsonPropertyName("np")]
        public double Np { get; set; }

        [JsonPropertyName("npl")]
        public double Npl { get; set; }

        [JsonPropertyName("npla")]
        public double Npla { get; set; }

        [JsonPropertyName("op")]
        public double Op { get; set; }

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

        [JsonPropertyName("rpnl")]
        public double Rpnl { get; set; }

        [JsonPropertyName("upnl")]
        public double Upnl { get; set; }

        [JsonPropertyName("info")]
        public Info Info { get; set; }

        [JsonPropertyName("event")]
        public string Event { get; set; }
    }
}
