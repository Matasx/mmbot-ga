internal class StrategyWrapper : IStrategy
{
    private IStrategy _strategy;

    public StrategyWrapper(IStrategy strategy)
    {
        _strategy = strategy;
    }

    public ChartPoint CalcChart(double price)
    {
        return _strategy.CalcChart(price);
    }

    public double CalcCurrencyAllocation(double price)
    {
        return _strategy.CalcCurrencyAllocation(price);
    }

    public double CalcInitialPosition(MarketInfo minfo, double price, double assets, double currency)
    {
        return _strategy.CalcInitialPosition(minfo, price, assets, currency);
    }

    public MinMax CalcSafeRange(MarketInfo minfo, double assets, double currencies)
    {
        return _strategy.CalcSafeRange(minfo, assets, currencies);
    }

    public string DumpStatePretty(MarketInfo minfo)
    {
        return _strategy.DumpStatePretty(minfo);
    }

    public string ExportState()
    {
        return _strategy.ExportState();
    }

    public BudgetInfo GetBudgetInfo()
    {
        return _strategy.GetBudgetInfo();
    }

    public double GetCenterPrice(double lastPrice, double assets)
    {
        return _strategy.GetCenterPrice(lastPrice, assets);
    }

    public double GetEquilibrium(double assets)
    {
        return _strategy.GetEquilibrium(assets);
    }

    public string GetId()
    {
        return _strategy.GetId();
    }

    public OrderData GetNewOrder(MarketInfo minfo, double curPrice, double newPrice, double dir, double assets, double currency, bool rej)
    {
        return _strategy.GetNewOrder(minfo, curPrice, newPrice, dir, assets, currency, rej);
    }

    public IStrategy ImportState(string src, MarketInfo minfo)
    {
        return _strategy.ImportState(src, minfo);
    }

    public bool IsValid()
    {
        return _strategy != null && _strategy.IsValid();
    }

    public IStrategy OnIdle(MarketInfo minfo, Ticker curTicker, double assets, double currency)
    {
        return _strategy = _strategy.OnIdle(minfo, curTicker, assets, currency);
    }

    public (OnTradeResult Result, IStrategy Strategy) OnTrade(MarketInfo minfo, double tradePrice, double tradeSize, double assetsLeft, double currencyLeft)
    {
        return _strategy.OnTrade(minfo, tradePrice, tradeSize, assetsLeft, currencyLeft);
    }

    public IStrategy Reset()
    {
        return _strategy.Reset();
    }
}