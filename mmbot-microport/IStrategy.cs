internal interface IStrategy
{
    bool IsValid() ;
    IStrategy OnIdle(MarketInfo minfo, Ticker curTicker, double assets, double currency) ;
    (OnTradeResult Result, IStrategy Strategy) OnTrade(MarketInfo minfo, double tradePrice, double tradeSize, double assetsLeft, double currencyLeft);
    string ExportState();
    string DumpStatePretty(MarketInfo minfo);
    IStrategy ImportState(string src, MarketInfo minfo);
    OrderData GetNewOrder(MarketInfo minfo, double curPrice, double newPrice, double dir, double assets, double currency, bool rej) ;
    MinMax CalcSafeRange(MarketInfo minfo, double assets, double currencies) ;
    double GetEquilibrium(double assets) ;
    IStrategy Reset();
    string GetId();
    double CalcInitialPosition(MarketInfo minfo, double price, double assets, double currency);
    BudgetInfo GetBudgetInfo() ;
    double CalcCurrencyAllocation(double price) ;
    ChartPoint CalcChart(double price) ;
    double GetCenterPrice(double lastPrice, double assets) ;
}