using System.Text.Json.Serialization;

namespace MMBotGA.dto
{
    public class Config
    {
        [JsonPropertyName("strategy")]
        public IStrategy Strategy { get; set; }

        [JsonPropertyName("broker")]
        public string Broker { get; set; }

        [JsonPropertyName("pair_symbol")]
        public string PairSymbol { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("dry_run")]
        public bool DryRun { get; set; }

        [JsonPropertyName("hidden")]
        public bool Hidden { get; set; }

        [JsonPropertyName("swap_symbols")]
        public bool SwapSymbols { get; set; }

        [JsonPropertyName("accept_loss")]
        public int AcceptLoss { get; set; }

        [JsonPropertyName("grant_trade_hours")]
        public int GrantTradeHours { get; set; }

        [JsonPropertyName("spread_calc_stdev_hours")]
        public double SpreadCalcStdevHours { get; set; }

        [JsonPropertyName("spread_calc_sma_hours")]
        public double SpreadCalcSmaHours { get; set; }

        [JsonPropertyName("dynmult_raise")]
        public double DynmultRaise { get; set; }

        [JsonPropertyName("dynmult_fall")]
        public double DynmultFall { get; set; }

        [JsonPropertyName("dynmult_mode")]
        public string DynmultMode { get; set; }

        [JsonPropertyName("dynmult_sliding")]
        public bool DynmultSliding { get; set; }

        [JsonPropertyName("spread_freeze")]
        public bool SpreadFreeze { get; set; }

        [JsonPropertyName("dynmult_mult")]
        public bool DynmultMult { get; set; }

        [JsonPropertyName("dynmult_cap")]
        public double DynmultCap { get; set; }

        [JsonPropertyName("buy_step_mult")]
        public double BuyStepMult { get; set; }

        [JsonPropertyName("sell_step_mult")]
        public double SellStepMult { get; set; }

        [JsonPropertyName("min_size")]
        public int MinSize { get; set; }

        [JsonPropertyName("max_size")]
        public int MaxSize { get; set; }

        [JsonPropertyName("max_leverage")]
        public int MaxLeverage { get; set; }

        [JsonPropertyName("secondary_order")]
        public int SecondaryOrder { get; set; }

        [JsonPropertyName("internal_balance")]
        public bool InternalBalance { get; set; }

        [JsonPropertyName("dont_allocate")]
        public bool DontAllocate { get; set; }

        [JsonPropertyName("report_order")]
        public int ReportOrder { get; set; }

        [JsonPropertyName("emulate_leveraged")]
        public int EmulateLeveraged { get; set; }

        [JsonPropertyName("adj_timeout")]
        public int AdjTimeout { get; set; }

        [JsonPropertyName("reduce_on_leverage")]
        public bool ReduceOnLeverage { get; set; }
    }
}
