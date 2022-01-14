using System.Text.Json.Serialization;

namespace MMBot.Api.dto
{
    public class RunRequest
    {
        [JsonPropertyName("config")]
        public Config Config { get; set; }

        [JsonPropertyName("minfo")]
        public Minfo Minfo { get; set; }

        [JsonPropertyName("init_price")]
        public double InitPrice { get; set; }

        [JsonPropertyName("init_pos")]
        public double? InitPos { get; set; }

        [JsonPropertyName("balance")]
        public double Balance { get; set; }

        [JsonPropertyName("fill_atprice")]
        public bool FillAtprice { get; set; }

        [JsonPropertyName("start_date")]
        public string StartDate { get; set; }

        [JsonPropertyName("reverse")]
        public bool Reverse { get; set; }

        [JsonPropertyName("invert")]
        public bool Invert { get; set; }

        [JsonPropertyName("neg_bal")]
        public bool NegBal { get; set; }

        [JsonPropertyName("spend")]
        public bool Spend { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }
    }
}
