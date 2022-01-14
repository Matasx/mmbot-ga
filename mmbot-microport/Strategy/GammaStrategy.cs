using System.Text.Json;
using System.Text.Json.Serialization;

namespace mmbot_microport.Strategy;

internal class GammaStrategy : IStrategy
{
    public enum Function
    {
        Halfhalf,
        Keepvalue,
        Exponencial,
        Gauss,
        Invsqrtsinh
    }

    public class IntegrationTable
    {
        public readonly Function Fn;
        public readonly double Z;
        public readonly double A;
        public readonly double B;
        private readonly List<Tuple<double, double>> _values;

        public IntegrationTable(Function fn, double z)
        {
            this.Fn = fn;
            this.Z = z;

            if (z <= 0.1) throw new ArgumentOutOfRangeException(nameof(z), z, "Invalid exponent value");
            double y;

            //calculate maximum for integration. Since this point, the integral is flat
            //power number 16 by 1/z (for z=1, this value is 16)
            B = Math.Pow(16, 1.0 / z);
            switch (fn)
            {
                case Function.Exponencial:
                case Function.Gauss:
                    A = 0;
                    y = 0; break;
                case Function.Invsqrtsinh:
                    A = Math.Pow(0.001, 1.0 / z); ;
                    y = 0; break;
                case Function.Halfhalf:
                    A = Math.Pow(0.0001, 1.0 / z);
                    y = 2 * Math.Sqrt(A);
                    break;
                default:
                    A = Math.Pow(0.0001, 1.0 / z);
                    y = 0;
                    break;
            }
            //generate integration table between a and b.
            //maximum step is 0.00001
            //starting by y and generate x,y table
            _values = new List<Tuple<double, double>>();
            Numerical.GenerateIntTable(MainFunction, A, B, 0.0001, y, (l, r) => _values.Add(new Tuple<double, double>(l, r)));
        }

        private double Get(double x)
        {
            if (x <= A)
            {
                return Fn switch
                {
                    Function.Halfhalf => 2 * Math.Sqrt(x),
                    Function.Invsqrtsinh => _values[0].Item2,
                    _ => Math.Log(x / A)
                };
            }

            //because table is ordered, use divide-half to search first  >= x;
            var i = _values.FindIndex(v => v.Item1 >= x && v.Item2 >= 0);
            switch (i)
            {
                //for the very first record, just return the value
                case 0:
                    return _values.First().Item2;
                //if we are after end, return last value
                case -1:
                    return _values.Last().Item2;
                default:
                    //retrieve lower bound
                    var (ll, lh) = _values[i - 1];
                    //retrieve upper bound
                    var (ul, uh) = _values[i];
                    //linear approximation
                    return lh + (uh - lh) * (x - ll) / (ul - ll);
            }
        }

        public double GetMax() => _values.Last().Item2;
        public double GetMin() => Math.Pow(0.000095, 1.0 / Z);

        private double MainFunction(double x)
        {
            return Fn switch
            {
                Function.Halfhalf => Math.Exp(-(Math.Pow(x, Z)) - 0.5 * Math.Log(x)),
                Function.Keepvalue => Math.Exp(-Math.Pow(x, Z)) / x,
                Function.Exponencial => Math.Exp(-Math.Pow(x, Z)),
                Function.Gauss => Math.Exp(-(x * x) - Math.Pow(x, Z)),
                Function.Invsqrtsinh => 1.0 / Math.Sqrt(Math.Sinh(Math.Pow(x * 1.2, Z))),
                _ => 0
            };
        }

        public double CalcAssets(double k, double w, double x)
        {
            return Fn switch
            {
                Function.Halfhalf => MainFunction(x / k) * w / k,
                Function.Keepvalue => MainFunction(x / k) * w / k,
                Function.Exponencial => MainFunction(x / k) * w / k,
                Function.Gauss => MainFunction(x / k) * w / k,
                Function.Invsqrtsinh => MainFunction(x / k) * w / k,
                _ => 0
            };
        }

        public double CalcBudget(double k, double w, double x)
        {
            return Fn switch
            {
                Function.Halfhalf => Get(x / k) * w,
                Function.Keepvalue => Get(x / k) * w,
                Function.Exponencial => Get(x / k) * w,
                Function.Gauss => Get(x / k) * w,
                Function.Invsqrtsinh => Get(x / k) * w,
                _ => 0
            };
        }

        double CalcCurrency(double k, double w, double x) => CalcBudget(k, w, x) - CalcAssets(k, w, x) * x;
    }

    private struct JsonConfig
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("function")]
        public string Function { get; set; }

        [JsonPropertyName("exponent")]
        public double Exponent { get; set; }

        [JsonPropertyName("rebalance")]
        public string Rebalance { get; set; }

        [JsonPropertyName("trend")]
        public double Trend { get; set; }

        [JsonPropertyName("reinvest")]
        public bool Reinvest { get; set; }
    }

    public record Config(
        IntegrationTable IntTable,
        int ReductionMode,
        double Trend,
        bool Reinvest,
        bool Maxrebalance
    )
    {
        public string CalcConfigHash()
        {
            return new Tuple<Function, double, double>(IntTable.Fn, IntTable.Z, Trend)
                .GetHashCode()
                .ToString();
        }

        public static Config FromJson(string config)
        {
            var cf = JsonSerializer.Deserialize<JsonConfig>(config);
            return new Config(
                new IntegrationTable(Enum.Parse<Function>(cf.Function, true), cf.Exponent),
                int.Parse(cf.Rebalance),
                cf.Trend,
                cf.Reinvest,
                false
            );
        }
    }

    public record State(
        double K = 0,
        double W = 0,
        double P = 0,
        double B = 0,
        double D = 0,
        double Uv = 0, //unprocessed volume
        double Kk = 0
    );

    public static string ID = "gamma";
    private readonly Config _cfg;
    private readonly State _state;

    public GammaStrategy(Config cfg, State state)
    {
        _cfg = cfg;
        _state = state;
    }

    public bool IsValid()
    {
        return _state.K > 0 && _state.P > 0 && _state.W > 0 && _state.B > 0;
    }

    public IStrategy OnIdle(MarketInfo minfo, Ticker curTicker, double assets, double currency)
    {
        return IsValid() ? this : Init(minfo, curTicker.Last, assets, currency);
    }

    private static double CalcMinOrderSize(MarketInfo minfo, double price)
    {
        return new[] {minfo.AssetStep, minfo.MinSize, minfo.MinVolume / price}.Max();
    }

    public (OnTradeResult Result, IStrategy Strategy) OnTrade(MarketInfo minfo, double tradePrice, double tradeSize,
        double assetsLeft, double currencyLeft)
    {
        if (!IsValid()) return Init(minfo, tradePrice, assetsLeft, currencyLeft)
            .OnTrade(minfo, tradePrice, tradeSize, assetsLeft, currencyLeft);

        var curPos = assetsLeft - tradeSize;

        var nn = CalculateNewNeutral(curPos, tradePrice, CalcMinOrderSize(minfo, tradePrice));
        if (tradeSize == 0 && Math.Abs(nn.K - tradePrice) > Math.Abs(_state.K - tradePrice))
        {
            nn = new NNRes(_state.K, _state.W);
        }
        var newkk = CalibK(nn.K);
        var volume = -tradePrice * tradeSize;
        var calcPos = _cfg.IntTable.CalcAssets(newkk, nn.W, tradePrice);
        var unprocessed = (assetsLeft - calcPos) * tradePrice;
        var prevCalcPos = _cfg.IntTable.CalcAssets(_state.Kk, _state.W, _state.P);
        var prevUnprocessed = (assetsLeft - tradeSize - prevCalcPos) * _state.P;
        var prevCur = _state.B - prevCalcPos * _state.P - prevUnprocessed;
        var bn = _cfg.IntTable.CalcBudget(newkk, nn.W, tradePrice);
        var newCur = bn - calcPos * tradePrice - unprocessed;
        var np = volume - newCur + prevCur;
        var neww = nn.W;
        var d = _state.D;

        if (_cfg.Reinvest && tradeSize != 0)
        {
            d += np;
            if (d > 0)
            {
                neww = nn.W * (bn + d) / bn;
                d = 0;
            }
            bn = _cfg.IntTable.CalcBudget(newkk, neww, tradePrice);
        }

        var nwst = new State(nn.K,neww,tradePrice, bn,d,unprocessed,newkk);
        return new ValueTuple<OnTradeResult, IStrategy>(
            new OnTradeResult(np, 0, nn.K, 0),
            new GammaStrategy(_cfg, nwst)
        );
    }

    public string ExportState()
    {
        return JsonSerializer.Serialize(new Dictionary<string, double>
        {
            {"k",_state.K},
            {"w",_state.W},
            {"p",_state.P},
            {"b",_state.B},
            {"d",_state.D},
            {"uv",_state.Uv},
            //{"hash",cfg.calcConfigHash()}
        });
    }

    public string DumpStatePretty(MarketInfo minfo)
    {
        var inv = minfo.InvertPrice;
        return JsonSerializer.Serialize(new Dictionary<string, double>
        {
            {"Position", (inv ? -1.0 : 1.0) * _cfg.IntTable.CalcAssets(_state.Kk, _state.W, _state.P)},
            {"Price.neutral", inv ? 1.0 / _state.Kk : _state.Kk},
            {"Price.last", inv ? 1.0 / _state.P : _state.P},
            {"Budget.max", _cfg.IntTable.GetMax() * _state.W},
            {"Budget.current", _state.B},
            {"Budget.not_traded", _state.Uv}
        });
    }

    public IStrategy ImportState(string src, MarketInfo minfo)
    {
        var data = JsonSerializer.Deserialize<Dictionary<string, double>>(src);
        return new GammaStrategy(_cfg, new State(
            0,
            data["w"],
            data["p"],
            data["b"],
            data["d"],
            data["uv"]
        ));
    }

    private static double MinSize(MarketInfo minfo, double price)
    {
        return new[]
        {
            minfo.MinSize,
            minfo.MinVolume / price,
            minfo.AssetStep
        }.Max();
    }

    static double RoundZero(double finpos, MarketInfo minfo, double price)
    {
        var afinpos = Math.Abs(finpos);
        if (afinpos < MinSize(minfo, price)) return 0;
        return finpos;
    }

    public OrderData GetNewOrder(MarketInfo minfo, double curPrice, double newPrice, double dir, double assets, double currency,
        bool rej)
    {
        var newPos = CalculatePosition(assets, newPrice, CalcMinOrderSize(minfo, newPrice));
        var newPosz = RoundZero(newPos, minfo, newPrice);
        var dff = newPosz - assets;
        var dffz = RoundZero(dff, minfo, newPrice);
        return dir switch
        {
            < 0 when dffz == 0 && newPosz == 0 => new OrderData(newPrice, 0, AlertType.Forced),
            > 0 when dffz == 0 && newPosz == 0 => new OrderData(0, MinSize(minfo, newPrice)),
            _ => new OrderData(0, dff)
        };
    }

    public MinMax CalcSafeRange(MarketInfo minfo, double assets, double currencies)
    {
        var a = CalculateCurPosition();
        double max, min;
        if (a > assets)
        {
            if (assets < 0) max = _state.P;
            else
                max = Numerical.NumericSearchR2(_state.P,
                    p => _cfg.IntTable.CalcAssets(_state.Kk, _state.W, p) - a + assets);
        }
        else
        {
            max = double.PositiveInfinity;
        }
        var cur = _cfg.IntTable.CalcBudget(_state.Kk, _state.W, _state.P) - a * _state.P;
        var adjcur = minfo.Leverage != 0 ? minfo.Leverage * currencies - assets * _state.P : currencies;
        if (cur > adjcur || _cfg.IntTable.Fn == Function.Keepvalue)
        {
            if (adjcur < 0) min = _state.P;
            else
                min = Numerical.NumericSearchR1(_state.P, p => _cfg.IntTable.CalcBudget(_state.Kk, _state.W, p)
                    - _cfg.IntTable.CalcAssets(_state.Kk, _state.W, p) * _state.P
                    - cur + adjcur);
        }
        else min = 0;

        return new MinMax(min, max);
    }

    public double GetEquilibrium(double assets)
    {
        var a = _cfg.IntTable.CalcAssets(_state.Kk, _state.W, _state.P);
        if (assets > a)
        {
            return Numerical.NumericSearchR1(_state.P,
                price => _cfg.IntTable.CalcAssets(_state.Kk, _state.W, price) - assets);
        }

        if (assets < a)
        {
            return Numerical.NumericSearchR2(_state.P,
                price => _cfg.IntTable.CalcAssets(_state.Kk, _state.W, price) - assets);
        }

        return _state.P;
    }

    public IStrategy Reset() => new GammaStrategy(_cfg, new State());

    public string GetId() => ID;

    public double CalcInitialPosition(MarketInfo minfo, double price, double assets, double currency)
    {
        var budget = price * assets + currency;
        var kk = CalibK(price);
        var normb = _cfg.IntTable.CalcBudget(kk, 1, price);
        var w = budget / normb;
        return _cfg.IntTable.CalcAssets(kk, w, price);
    }

    public BudgetInfo GetBudgetInfo()
    {
        return new BudgetInfo(
            _cfg.IntTable.CalcBudget(_state.Kk, _state.W, _state.P),
            _cfg.IntTable.CalcAssets(_state.Kk, _state.W, _state.P)
        );
    }

    public double CalcCurrencyAllocation(double price)
    {
        return _cfg.IntTable.CalcBudget(_state.Kk, _state.W, _state.P)
               - _cfg.IntTable.CalcAssets(_state.Kk, _state.W, _state.P) * _state.P
               - _state.Uv;
    }

    public ChartPoint CalcChart(double price)
    {
        var a = _cfg.IntTable.CalcAssets(_state.Kk, _state.W, price);
        var b = _cfg.IntTable.CalcBudget(_state.Kk, _state.W, price);
        if (b < 0) b = 0;
        return new ChartPoint(
            true,
            a,
            b
        );
    }

    public double GetCenterPrice(double lastPrice, double assets)
    {
        return assets == 0 ? lastPrice : GetEquilibrium(assets);
    }

    private double CalculatePosition(double a, double price, double minsize)
    {
        var newk = CalculateNewNeutral(a, price, minsize);
        var newkk = CalibK(newk.K);
        return _cfg.IntTable.CalcAssets(newkk, newk.W, price);
    }

    private GammaStrategy Init(MarketInfo minfo, double price, double assets, double currency)
    {
        var budget = _state.B > 0 ? _state.B : assets * price + currency;
        if (budget <= 0) throw new InvalidOperationException("No budget");
        if (_state.P != 0) price = _state.P;
        var newstP = price;
        var newstK = 0d;
        if (newstP <= 0) throw new InvalidOperationException("Invalid price");
        if (assets <= 0) newstK = price;
        else
        {
            var r = assets * price / budget;
            var k = price / _cfg.IntTable.B; //assume that k is lowest possible value;
            var a = _cfg.IntTable.CalcAssets(CalibK(k), 1, price);
            var b = _cfg.IntTable.CalcBudget(CalibK(k), 1, price);
            var r0 = a / b * price;
            if (r > r0)
            {
                if (r < 0.5 || _cfg.IntTable.Fn != Function.Halfhalf)
                {
                    newstK = Numerical.NumericSearchR2(k, k1 => {
                        var a1 = _cfg.IntTable.CalcAssets(CalibK(k1), 1, price);
                        var b1 = _cfg.IntTable.CalcBudget(CalibK(k1), 1, price);
                        if (b1 <= 0) return double.MaxValue;
                        if (a1 <= 0) return 0.0;
                        return a1 / b1 * price - r;
                    });
                }
                else
                {
                    newstK = (price / _cfg.IntTable.A) / CalibK(1.0);
                    budget = 2 * assets * price;
                }
            }
            else
            {
                newstK = price;
            }
        }
        var newstKk = CalibK(newstK);
        var w1 = _cfg.IntTable.CalcBudget(newstKk, 1.0, price);

        var newst = new State
        {
            P = newstP,
            K = newstK,
            Kk = newstKk,
            W = budget / w1,
            B = budget,
            D = 0,
            Uv = 0
        };

        var strategy = new GammaStrategy(_cfg, newst);
        if (!strategy.IsValid()) throw new InvalidOperationException("Failed to initialize strategy");
        return strategy;
    }

    private double CalculateCurPosition()
    {
        return _cfg.IntTable.CalcAssets(_state.Kk, _state.W, _state.P);
    }

    private record NNRes(
        double K,
        double W
    );

    private NNRes CalculateNewNeutral(double a, double price, double min_order_size)
    {
        if ((price - _state.K) * (_state.P - _state.K) < 0)
        {
            return new NNRes(_state.K, _state.W);
        }
        var pnl = a * (price - _state.P);
        var w = _state.W;
        var mode = _cfg.ReductionMode;
        if (price < _state.K && !_cfg.Maxrebalance && (mode == 0
                                                       || (mode == 1 && price > _state.P)
                                                       || (mode == 2 && price < _state.P))) return new NNRes(_state.K,w);

        double bc;
        double needb;
        double newk;
        if (price > _state.K)
        {

            if (price < _state.P && _cfg.Maxrebalance)
            {
                bc = _cfg.IntTable.CalcBudget(_state.Kk, _state.W, _state.P);
                needb = pnl + bc;
                w = Numerical.NumericSearchR2(0.5 * _state.W,
                    w1 => _cfg.IntTable.CalcBudget(_state.Kk, w1, price) - needb);
                newk = _state.K;
            }
            else
            {
                bc = _cfg.IntTable.CalcBudget(_state.Kk, _state.W, price);
                needb = bc - pnl;
                newk = Numerical.NumericSearchR2(0.5 * _state.K,
                    k => _cfg.IntTable.CalcBudget(CalibK(k), _state.W, _state.P) - needb);
                if (newk < _state.K /*&& cfg.intTable->calcAssets(newk, state.w, price)<min_order_size*/)
                    newk = (3 * _state.K + price) * 0.25;
            }
        }
        else if (price < _state.K)
        {
            if (_cfg.Maxrebalance && price > _state.P)
            {
                var k = price * 0.1 + _state.K * 0.9;//*cfg.intTable->GetMin();
                var kk = CalibK(k);
                var w1 = _cfg.IntTable.CalcAssets(kk, 1.0, price);
                var w2 = _cfg.IntTable.CalcAssets(_state.Kk, _state.W, price);
                var neww = w2 / w1;
                if (neww > w * 2)
                {
                    return new NNRes(_state.K, _state.W);
                }
                w = neww;
                newk = k;
                //var pos1 = cfg.intTable.calcAssets(state.kk, state.w, price);
                //var pos2 = cfg.intTable.calcAssets(kk, w, price);
                //var b1 = cfg.intTable.calcBudget(state.kk, state.w, price);
                //var b2 = cfg.intTable.calcBudget(kk, w, price);
                //logDebug("Rebalance POS: $1 > $2, BUDGET: $3 > $4", pos1, pos2, b1, b2);
            }
            else
            {
                bc = _cfg.IntTable.CalcBudget(_state.Kk, _state.W, _state.P);
                needb = bc + pnl;
                if (mode == 4 && price / _state.Kk > 1.0)
                {
                    var spr = price / _state.P;
                    var reff = _cfg.IntTable.CalcAssets(_state.Kk, _state.W, _state.K) * _state.K * (spr - 1.0)
                        + _cfg.IntTable.CalcBudget(_state.Kk, _state.W, _state.K) - _cfg.IntTable.CalcBudget(_state.Kk, _state.W, _state.K * spr);
                    //double bq = cfg.intTable->calcBudget(state.kk, state.w, price);
                    //				double maxref = needb - bq;
                    //ref = std::min(ref, maxref);
                    needb -= reff;
                }

                newk = Numerical.NumericSearchR1(1.5 * _state.K,
                    k => _cfg.IntTable.CalcBudget(CalibK(k), _state.W, price) - needb);
            }
        }
        else
        {
            newk = _state.K;
        }
        if (newk < 1e-100 || newk > 1e+100) newk = _state.K;
        return new NNRes(newk, w);
    }

    private double CalibK(double k)
    {
        if (_cfg.Maxrebalance) return k / _cfg.IntTable.GetMin();
        var l = -_cfg.Trend / 100.0;
        var kk = Math.Pow(Math.Exp(-1.6 * l * l + 3.4 * l), 1.0 / _cfg.IntTable.Z);
        return k / kk;
    }
}