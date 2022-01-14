internal class DynMultControl
{
    public record Config(
        double Raise,
        double Fall,
        double Cap,
        DynmultModeType ModeType,
        bool Mult
    );

    public DynMultControl(Config config)
    {
        _cfg = config;
    }

    private void SetMult(double buy, double sell)
    {
        _multBuy = buy;
        _multSell = sell;
    }

    public double GetBuyMult() => _multBuy;
    public double GetSellMult() => _multSell;

    private double raise_fall(double v, bool israise)
    {
        if (israise)
        {
            var rr = _cfg.Raise / 100.0;
            return Math.Min(_cfg.Mult ? v * (1 + rr) : v + rr, _cfg.Cap);
        }
        else
        {
            var ff = _cfg.Fall / 100.0;
            return Math.Max(1.0, _cfg.Mult ? v * (1.0 - ff) : v - ff);
        }
    }

    public void Update(bool buyTrade, bool sellTrade)
    {
        switch (_cfg.ModeType)
        {
            case DynmultModeType.Disabled:
                _multBuy = 1.0;
                _multSell = 1.0;
                return;
            case DynmultModeType.Independent:
                break;
            case DynmultModeType.Together:
                buyTrade = buyTrade || sellTrade;
                sellTrade = buyTrade;
                break;
            case DynmultModeType.Alternate:
                if (buyTrade) _multSell = 0;
                else if (sellTrade) _multBuy = 0;
                break;
            case DynmultModeType.Half_alternate:
                if (buyTrade) _multSell = ((_multSell - 1) * 0.5) + 1;
                else if (sellTrade) _multBuy = ((_multBuy - 1) * 0.5) + 1;
                break;
        }
        _multBuy = raise_fall(_multBuy, buyTrade);
        _multSell = raise_fall(_multSell, sellTrade);
    }

    private void Reset()
    {
        _multBuy = 1;
        _multSell = 1;
    }

    private readonly Config _cfg;
    private double _multBuy = 1;
    private double _multSell = 1;

}