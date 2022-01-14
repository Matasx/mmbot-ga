using System.Text.Json;
using MMBot.Api;
using MMBot.Api.dto;
using mmbot_microport.utils;

public class MMBotApiProxy : IMMBotApi
{
    private readonly Cache<string, object> _data = new(30);
    private readonly IMMBotApi _botInstance;

    public MMBotApiProxy(IMMBotApi botInstance)
    {
        _botInstance = botInstance;
    }


    public Task<Minfo> GetInfoAsync(string broker, string pair)
    {
        return _botInstance.GetInfoAsync(broker, pair);
    }

    public Task<FileIdResponse> UploadAsync(string data)
    {
        var id = Guid.NewGuid().ToString();
        _data.Set(id, data);
        return Task.FromResult(new FileIdResponse {Id = id});
    }

    public Task<string> GetFileAsync(GetFileRequest request)
    {
        var (success, value) = _data.Get(request.Source);
        if (success) return Task.FromResult((string)value);
        throw new ArgumentOutOfRangeException(nameof(request.Source), "File is missing.");
    }

    public Task<FileIdResponse> GenerateTradesAsync(GenTradesRequest request)
    {
        List<double> srcMinute;
        var parsedId = request.Source + "-parsed";
        var (success, parsedValue) = _data.Get(parsedId);
        if (!success)
        {
            var (success2, value) = _data.Get(request.Source);
            if (!success2)
            {
                throw new ArgumentOutOfRangeException(nameof(request.Source), "File is missing.");
            }

            srcMinute = JsonSerializer.Deserialize<List<double>>((string)value);

            //using var sr = new StringReader((string)value);
            //string line;
            //while ((line = await sr.ReadLineAsync()) != null)
            //{
            //    srcMinute.Add(double.Parse(line, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture));
            //}

            _data.Set(parsedId, srcMinute);
        }
        else
        {
            srcMinute = (List<double>)parsedValue;
        }
        
        var fn = new DefaulSpread(request.Sma, request.Stdev, request.ForceSpread);
        var spreadCalc = new VisSpread(fn, new VisSpread.Config(
            new DynMultControl.Config(
                request.Raise,
                request.Fall,
                request.Cap,
                Enum.Parse<DynmultModeType>(request.Mode, true),
                request.DynMult
            ),
            request.Mult,
            request.Order2,
            request.Sliding,
            request.SpreadFreeze
        ));
        if (request.Reverse)
        {
            srcMinute = new List<double>(srcMinute);
            srcMinute.Reverse();
        }

        var t = request.BeginTime ?? UnixEpoch.GetEpochMs(DateTime.UtcNow.AddMinutes(-srcMinute.Count));
        var ofs = request.Offset ?? 0;
        var lim = Math.Min(request.Limit ?? srcMinute.Count, srcMinute.Count - ofs);

        var result = new List<BTPrice>();
        BTPrice last = null;
        var init = 0d;
        foreach (var itm in srcMinute.Skip(ofs).Take(lim))
        {
            var w = itm;
            if (request.Invert)
            {
                if (init == 0) init = w * w;
                w = init / w;
            }

            var v = w;
            if (request.Ifutures)
            {
                v = 1 / v;
            }

            var res = spreadCalc.Point(v);
            if (res.Trade != 0 && res.Valid)
            {
                var p = request.Ifutures ? 1.0 / res.Price : res.Price;
                result.Add(last = new BTPrice(t, p, p, p));
                if (res.Trade2 != 0)
                {
                    p = request.Ifutures ? 1.0 / res.Price2 : res.Price2;
                    result.Add(last = new BTPrice(t, p, p, p));
                }
            }
            else if (w < last?.Pmin)
            {
                last = last with
                {
                    Pmin = w
                };
            }
            else if (w > last?.Pmax)
            {
                last = last with
                {
                    Pmax = w
                };
            }

            t += 60000;
        }

        var id = Guid.NewGuid().ToString();
        _data.Set(id, result);
        return Task.FromResult(new FileIdResponse
        {
            Id = id
        });
    }

    public Task<IList<RunResponse>> RunAsync(RunRequest request)
    {
        if (request.Minfo == null) throw new ArgumentNullException(nameof(request.Minfo));

        var mconfig = new MTraderConfig(
            request.Config.PairSymbol, 
            request.Config.Broker, 
            request.Config.Title, 
            request.Config.BuyStepMult,
            request.Config.SellStepMult,
            request.Config.MinSize,
            request.Config.MaxSize)
        {
            //MinBalance,
            //MaxBalance,
            AcceptLoss = (uint)request.Config.AcceptLoss,
            MaxLeverage = request.Config.MaxLeverage,
            ReduceOnLeverage = request.Config.ReduceOnLeverage,
            //TradeWithinBudget,
            Strategy = StrategyFactory.Create(request.Config.Strategy.Type, JsonSerializer.Serialize(request.Config.Strategy))
        };

        var (success, value) = _data.Get(request.Source);
        if (!success)
        {
            throw new ArgumentOutOfRangeException(nameof(request.Source), "File is missing.");
        }
        IEnumerable<BTPrice> trades = (List<BTPrice>)value;

        if (request.Reverse)
        {
            throw new NotSupportedException("Run reverse is not supported");
        }

        var mlt = 1d;
        var avg = trades.Average(x => x.Price);
        var fv = trades.FirstOrDefault()?.Price ?? request.InitPrice;
        if (request.InitPrice != 0 && trades.Any())
        {
            if (request.Invert) fv = 2 * avg - fv;
            mlt = request.InitPrice / fv;
            fv *= mlt;
        }

        if (mlt != 1d)
        {
            trades = trades.Select(x => x with
            {
                Price = x.Price * mlt,
                Pmin = x.Pmin * mlt,
                Pmax = x.Pmax * mlt
            });
        }

        if (request.Invert)
        {
            trades = trades.Select(x =>
            {
                var fv2 = fv * fv;
                return x with
                {
                    Price = fv2 / x.Price,
                    Pmin = fv2 / x.Pmax,
                    Pmax = fv2 / x.Pmin
                };
            });
        }

        if (request.Minfo.InvertPrice)
        {
            trades = trades.Select(x => x with
            {
                Price = 1 / x.Price,
                Pmin = 1 / x.Pmax,
                Pmax = 1 / x.Pmin
            });
        }

        var minfo = new MarketInfo(
            request.Minfo.AssetSymbol,
            request.Minfo.CurrencySymbol,
            request.Minfo.AssetStep,
            request.Minfo.CurrencyStep,
            request.Minfo.MinSize,
            request.Minfo.MinVolume,
            request.Minfo.Fees,
            Enum.Parse<FeeScheme>(request.Minfo.FeeScheme, true),
            request.Minfo.Leverage,
            request.Minfo.InvertPrice,
            request.Minfo.InvertedSymbol,
            request.Minfo.Simulator,
            request.Minfo.PrivateChart,
            request.Minfo.WalletId
        );
        var rs = Backtest.Cycle(mconfig, trades.ToList(), minfo, request.InitPos, request.Balance, request.NegBal,
            request.Spend);

        //todo: acb
        var result = rs.Select(x => new RunResponse
        {
            Np = x.NeutralPrice,
            //Op = x.,
            //rpnl
            //upnl
            Na = x.NormAccum,
            Npl = x.NormProfit,
            Npla = x.NormProfitTotal,
            Pl = x.Pl,
            Ps = x.Pos,
            Pr = x.Price,
            Tm = x.Time,
            Bal = x.Bal,
            Ubal = x.UnspendBalance,
            Info = JsonSerializer.Deserialize<Info>(x.Info ?? "{}"),
            Sz = x.Size
            //event
        });

        return Task.FromResult<IList<RunResponse>>(result.ToList());
    }
}