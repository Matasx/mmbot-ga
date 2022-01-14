using System.Text.Json.Serialization;

namespace MMBot.Api.dto
{
    public class Info
    {
        [JsonPropertyName("Budget.current")]
        public double BudgetCurrent { get; set; }

        [JsonPropertyName("Budget.max")]
        public double BudgetMax { get; set; }

        [JsonPropertyName("Budget.not_traded")]
        public double BudgetNotTraded { get; set; }

        [JsonPropertyName("Position")]
        public double Position { get; set; }

        [JsonPropertyName("Price.last")]
        public double PriceLast { get; set; }

        [JsonPropertyName("Price.neutral")]
        public double PriceNeutral { get; set; }
    }
}
