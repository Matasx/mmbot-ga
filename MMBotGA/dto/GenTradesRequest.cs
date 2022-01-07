using System.Text.Json.Serialization;

namespace MMBotGA.dto
{
    public class GenTradesRequest
    {
        /// <summary>
        /// Spread width: smooth hours
        /// </summary>
        [JsonPropertyName("sma")]
        public double Sma { get; set; }

        /// <summary>
        /// Spread width: st.dev hours
        /// </summary>
        [JsonPropertyName("stdev")]
        public double Stdev { get; set; }

        [JsonPropertyName("force_spread")]
        public int ForceSpread { get; set; }

        [JsonPropertyName("mult")]
        public double Mult { get; set; }

        [JsonPropertyName("raise")]
        public double Raise { get; set; }

        [JsonPropertyName("cap")]
        public double Cap { get; set; }

        [JsonPropertyName("fall")]
        public double Fall { get; set; }

        [JsonPropertyName("mode")]
        public string Mode { get; set; }

        [JsonPropertyName("sliding")]
        public bool Sliding { get; set; }

        [JsonPropertyName("spread_freeze")]
        public bool SpreadFreeze { get; set; }

        [JsonPropertyName("dyn_mult")]
        public bool DynMult { get; set; }

        [JsonPropertyName("reverse")]
        public bool Reverse { get; set; }

        [JsonPropertyName("invert")]
        public bool Invert { get; set; }

        [JsonPropertyName("ifutures")]
        public bool Ifutures { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("order2")]
        public int Order2 { get; set; }

        [JsonPropertyName("offset")]
        public int? Offset { get; set; }

        [JsonPropertyName("limit")]
        public int? Limit { get; set; }

        [JsonPropertyName("begin_time")]
        public long? BeginTime { get; set; }
    }
}
