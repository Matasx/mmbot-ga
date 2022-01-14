using System.Text.Json.Serialization;

namespace mmbot_microport.Strategy;

internal class KeepValueStrategy : IStrategy
{
    public static string ID = "keepvalue";

    private readonly Config _cfg;
    private readonly State _st;

    public KeepValueStrategy(Config cfg) : this (cfg, new State())
    {
    }

    public KeepValueStrategy(Config cfg, State st)
    {
        _cfg = cfg;
        _st = st;
    }

    public record Config(
        double Ea,
        double Accum,
        double Chngtm //JSON: valinc
    )
    {
        public static Config FromJson(string json)
        {
            throw new NotSupportedException();
        }
    }

    public record State(
        bool Valid = false,
        double P = 0,
        double A = 0,
        double C = 0,
        double F = 0,
        long RecalcTime = 0,
        long CheckTime = 0
    );

    public bool IsValid()
    {
        return _st.Valid && _st.P > 0;
    }

    public IStrategy OnIdle(MarketInfo minfo, Ticker curTicker, double assets, double currency)
    {
        if (IsValid())
        {
            if (_cfg.Chngtm == 0) return this;

            var nst = _st with
            {
                CheckTime = curTicker.Time,
                RecalcTime = _st.RecalcTime == 0 ? curTicker.Time : _st.RecalcTime
            };
            return new KeepValueStrategy(_cfg, nst);
        }

        if (assets + _cfg.Ea <= 0)
        {
            assets = currency / (2 * curTicker.Last) - _cfg.Ea;
        }

        return new KeepValueStrategy(_cfg,
            new State(true, curTicker.Last, assets, currency, currency, curTicker.Time, curTicker.Time));
    }

    public (OnTradeResult Result, IStrategy Strategy) OnTrade(MarketInfo minfo, double tradePrice, double tradeSize,
        double assetsLeft, double currencyLeft)
    {
        if (!IsValid())
        {
            var tmp = new KeepValueStrategy(_cfg, new State(true, tradePrice, assetsLeft - tradeSize));
            return tmp.OnTrade(minfo, tradePrice, tradeSize, assetsLeft, currencyLeft);
        }

        var k = CalcK();

        var accum = CalcAccumulation(tradePrice, currencyLeft);
        var norm = tradeSize == assetsLeft ? 0 : CalcNormalizedProfit(tradePrice, tradeSize);

        var nst = _st with
        {
            A = k / tradePrice + accum - _cfg.Ea,
            RecalcTime = _st.CheckTime,
            P = tradePrice,
            CheckTime = _st.CheckTime,
            Valid = true,
            F = currencyLeft,
            C = Math.Max(_st.C + CalcReqCurrency(tradePrice) - CalcReqCurrency(_st.P), 0.0)
        };

        return new ValueTuple<OnTradeResult, IStrategy>(
            new OnTradeResult(norm, accum),
            new KeepValueStrategy(_cfg, nst)
        );
    }

    private double CalcReqCurrency(double price)
    {
        var k = CalcK();
        return k * Math.Log(price / k) + _st.C;
    }

    private double CalcNormalizedProfit(double tradePrice, double tradeSize)
    {
        var cashFlow = -tradePrice * tradeSize;
        var oldCash = CalcReqCurrency(_st.P);
        var newCash = CalcReqCurrency(tradePrice);
        var diffCash = newCash - oldCash;
        return cashFlow - diffCash;
    }

    private double CalcAccumulation(double price, double currencyLeft)
    {
        if (_cfg.Accum == 0) return 0;

        var r1 = CalcReqCurrency(_st.P);
        var r2 = CalcReqCurrency(price);
        var pl = -price * (CalcA(price) - CalcA(_st.P));
        var nl = r2 - r1;
        var ex = pl - nl;
        return ex / price * _cfg.Accum;
    }

    private double CalcA(double price) => CalcK() / price;

    private double CalcK()
    {
        var k = _st.P * (_st.A + _cfg.Ea);
        var tm = (_st.CheckTime - _st.CheckTime);
        const double monthMsec = 24.0 * 60.0 * 60.0 * 30.0 * 1000.0;
        var f = tm / monthMsec;
        return Math.Max(k + _cfg.Chngtm * f, 0.0);
    }

    public string ExportState()
    {
        throw new NotImplementedException();
    }

    public string DumpStatePretty(MarketInfo minfo)
    {
        throw new NotImplementedException();
    }

    public IStrategy ImportState(string src, MarketInfo minfo)
    {
        throw new NotImplementedException();
    }

    public OrderData GetNewOrder(MarketInfo minfo, double curPrice, double newPrice, double dir, double assets, double currency,
        bool rej)
    {
        var k = CalcK();
        var na = k / newPrice + CalcAccumulation(newPrice, currency);
        return new OrderData(
            0,
            StrategyUtils.CalcOrderSize(_st.A, assets, na - _cfg.Ea)
        );
    }

    public MinMax CalcSafeRange(MarketInfo minfo, double assets, double currencies)
    {
        var k = CalcK();
        var n = _st.P * Math.Exp(-currencies / k);
        var m = _cfg.Ea > 0 ? k / _cfg.Ea : double.PositiveInfinity;
        return new MinMax(n, m);
    }

    public double GetEquilibrium(double assets) => CalcK() / (assets + _cfg.Ea);

    public IStrategy Reset()
    {
        return new KeepValueStrategy(_cfg, new State());
    }

    public string GetId() => ID;

    public double CalcInitialPosition(MarketInfo minfo, double price, double assets, double currency)
    {
        if (minfo.Leverage != 0) return currency / price * 0.5;
        return (assets + _cfg.Ea + currency / price) * 0.5;
    }

    public BudgetInfo GetBudgetInfo()
    {
        var cur = CalcReqCurrency(_st.P);
        return new BudgetInfo(cur + CalcK(), _st.A + _cfg.Ea);
    }

    public double CalcCurrencyAllocation(double price) => _st.C;

    public ChartPoint CalcChart(double price)
    {
        var a = CalcA(price);
        var k = CalcK();
        return new ChartPoint(true, a, k * Math.Log(price / _st.C) + a * price);
    }

    public double GetCenterPrice(double lastPrice, double assets) => GetEquilibrium(assets);
}