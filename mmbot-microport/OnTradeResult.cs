/// <param name="NormProfit">normalized profit</param>
/// <param name="NormAccum">normalized accumulated</param>
/// <param name="NeutralPrice">neutral position, if 0, the value is not drawn on chart</param>
/// <param name="OpenPrice">open price, if 0, the value is not drawn</param>
internal record OnTradeResult(
    double NormProfit,
    double NormAccum,
    double NeutralPrice = 0,
    double OpenPrice = 0);