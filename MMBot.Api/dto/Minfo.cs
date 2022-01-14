using System.Text.Json.Serialization;

namespace MMBot.Api.dto
{
    public class Minfo
    {
        [JsonPropertyName("asset_balance")]
        public double AssetBalance { get; set; }

        [JsonPropertyName("asset_step")]
        public double AssetStep { get; set; }

        [JsonPropertyName("asset_symbol")]
        public string AssetSymbol { get; set; }

        [JsonPropertyName("currency_balance")]
        public double CurrencyBalance { get; set; }

        [JsonPropertyName("currency_step")]
        public double CurrencyStep { get; set; }

        [JsonPropertyName("currency_symbol")]
        public string CurrencySymbol { get; set; }

        [JsonPropertyName("feeScheme")]
        public string FeeScheme { get; set; }

        [JsonPropertyName("fees")]
        public double Fees { get; set; }

        [JsonPropertyName("invert_price")]
        public bool InvertPrice { get; set; }

        [JsonPropertyName("inverted_symbol")]
        public string InvertedSymbol { get; set; }

        [JsonPropertyName("leverage")]
        public double Leverage { get; set; }

        [JsonPropertyName("min_size")]
        public double MinSize { get; set; }

        [JsonPropertyName("min_volume")]
        public double MinVolume { get; set; }

        [JsonPropertyName("price")]
        public double Price { get; set; }

        [JsonPropertyName("private_chart")]
        public bool PrivateChart { get; set; }

        [JsonPropertyName("quote_asset")]
        public string QuoteAsset { get; set; }

        [JsonPropertyName("quote_currency")]
        public string QuoteCurrency { get; set; }

        [JsonPropertyName("simulator")]
        public bool Simulator { get; set; }

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("wallet_id")]
        public string WalletId { get; set; }
    }
}
