internal class VisSpread
{
    public record Config(
        DynMultControl.Config Dynmult,
        double Mult = 1,
        double Order2 = 0,
        bool Sliding = false,
        bool Freeze = false
    );

    public record Result(
        bool Valid = false,
        double Price = 0, 
        double Low = 0, 
        double High = 0,
        int Trade = 0, //0=no trade, -1=sell, 1=buy
        double Price2 = 0, //price of secondary trade
        int Trade2 = 0  //0 = no secondary trade, -1=sell, 1=buy
    );

    public VisSpread(ISpreadFunction fn, Config cfg)
    {
        _fn = fn;
        _state = fn.Start();
        _dynmult = new DynMultControl(cfg.Dynmult);
        _sliding = cfg.Sliding;
        _freeze = cfg.Freeze;
        _mult = cfg.Mult;
        _order2 = cfg.Order2 * 0.01;
    }


    public Result Point(double y)
    {
        var sp = _fn.Point(_state, y);
        if (_lastPrice == 0)
        {
            _lastPrice = y;
            _offset = y;
            return new Result();
        }
        if (!sp.Valid) return new Result();

        var trade = 0;
        var trade2 = 0;
        var price = _lastPrice;
        double price2 = 0;

        var center = _sliding ? sp.Center : 0;
        if (y > _chigh)
        {
            var high2 = _chigh.Value * Math.Exp(_cspread * _order2);
            price = _chigh.Value;
            _lastPrice = _chigh.Value;
            _offset = _chigh.Value - center;
            trade = -1;
            if (_order2 != 0 && y > high2)
            {
                trade2 = -1;
                price2 = high2;
                _offset = high2 - center;
                _lastPrice = high2;
            }
            _dynmult.Update(false, true);
            /*if (frozen_side != -1)*/
            {
                _frozenSide = -1;
                _frozenSpread = _cspread;
            }
        }
        else if (y < _clow)
        {
            var low2 = _clow.Value * Math.Exp(-_cspread * _order2);
            price = _clow.Value;
            _lastPrice = _clow.Value;
            _offset = _clow.Value - center;
            trade = 1;
            if (_order2 != 0 && y < low2)
            {
                _lastPrice = low2;
                trade2 = 1;
                _offset = low2 - center;
                price2 = low2;
            }
            _dynmult.Update(true, false);
            /*if (frozen_side != 1)*/
            {
                _frozenSide = 1;
                _frozenSpread = _cspread;
            }
        }
        _dynmult.Update(false, false);

        var lspread = sp.Spread;
        var hspread = sp.Spread;
        if (_freeze)
        {
            if (_frozenSide < 0)
            {
                lspread = Math.Min(_frozenSpread, lspread);
            }
            else if (_frozenSide > 0)
            {
                hspread = Math.Min(_frozenSpread, hspread);
            }
        }
        var low = (center + _offset) * Math.Exp(-lspread * _mult * _dynmult.GetBuyMult());
        var high = (center + _offset) * Math.Exp(hspread * _mult * _dynmult.GetSellMult());
        if (_sliding && _lastPrice != 0)
        {
            var lowMax = _lastPrice * Math.Exp(-lspread * 0.01);
            var highMin = _lastPrice * Math.Exp(hspread * 0.01);
            if (low > lowMax)
            {
                high = lowMax + (high - low);
                low = lowMax;
            }
            if (high < highMin)
            {
                low = highMin - (high - low);
                high = highMin;

            }
            low = Math.Min(lowMax, low);
            high = Math.Max(highMin, high);
        }
        low = Math.Min(low, y);
        high = Math.Max(high, y);
        _chigh = high;
        _clow = low;
        _cspread = sp.Spread;
        return new Result(true, price, low, high, trade, price2, trade2);
    }

    private ISpreadFunction _fn;
    private object _state;
    private DynMultControl _dynmult;
    private bool _sliding;
    private bool _freeze;
    private double _mult;
    private double _order2;
    private double _offset = 0;
    private double _lastPrice = 0;
    private double? _chigh, _clow;
    private double _cspread;
    private int _frozenSide = 0;
    private double _frozenSpread = 0;
}