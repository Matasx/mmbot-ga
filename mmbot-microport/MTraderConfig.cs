internal record MTraderConfig(
    string Pairsymb,
    string Broker,
    string Title,

    double BuyStepMult,
    double SellStepMult,
    double MinSize,
    double MaxSize,
    double? MinBalance = null,
    double? MaxBalance = null,
    double? MaxCosts = null,

    double DynmultRaise = 0,
    double DynmultFall = 0,
    double DynmultCap = 0,
    DynmultMode DynmultMode = DynmultMode.Disabled,

    uint AcceptLoss = 0,
    uint AdjTimeout = 0,

    double ForceSpread = 0,
    double ReportOrder = 0,
    double MaxLeverage = 0,
    double EmulateLeveraged = 0,
    double SecondaryOrderDistance = 0,
    uint GrantTradeMinutes = 0,

    double SpreadCalcStdevHours = 0,
    double SpreadCalcSmaHours = 0,

    double InitOpen = 0,

    bool DryRun = false,
    bool InternalBalance = false,
    bool DontAllocate = false,
    bool Enabled = false,
    bool Hidden = false,
    bool DynmultSliding = false,
    bool DynmultMult = false,
    bool SwapSymbols = false,
    bool ReduceOnLeverage = false,
    bool FreezeSpread = false,
    bool TradeWithinBudget = false,

    IStrategy Strategy = null
)
{
    //void LoadConfig(json::Value data, bool forceDryRun);
}