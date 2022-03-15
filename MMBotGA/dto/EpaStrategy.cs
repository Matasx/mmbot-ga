using System.Text.Json.Serialization;

namespace MMBotGA.dto
{
    public class EpaStrategy : IStrategy
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("exit_power_mult")]
        public double ExitPowerMult { get; set; }

        [JsonPropertyName("initial_bet_perc_of_budget")]
        public double InitialBetPercOfBudget { get; set; }

        [JsonPropertyName("max_enter_price_distance")]
        public double MaxEnterPriceDistance { get; set; }

        [JsonPropertyName("min_asset_perc_of_budget")]
        public double MinAssetPercOfBudget { get; set; }

        [JsonPropertyName("power_cap")]
        public double PowerCap { get; set; }

        [JsonPropertyName("power_mult")]
        public double PowerMult { get; set; }

        [JsonPropertyName("target_exit_price_distance")]
        public double TargetExitPriceDistance { get; set; }

        [JsonPropertyName("angle")]
        public double Angle { get; set; }
    }
}