using System.Text.Json;

internal static class Backtest
{
    public static IEnumerable<BTTrade> Cycle(MTraderConfig cfg, ICollection<BTPrice> priceSource, MarketInfo minfo, double? initPos, double balance, bool negBal, bool spend)
    {
        IList<BTTrade> trades = new List<BTTrade>();
        var price1 = priceSource.FirstOrDefault();
        if (price1 == null) return trades;

        var s = cfg.Strategy;

        var btPrice = price1.Price;
        var btTime = price1.Time;
        var btSize = 0d;

        double pos;
        if (initPos.HasValue)
        {
            pos = initPos.Value;
            if (minfo.InvertPrice) pos = -pos;
        }
        else
        {
            pos = s.CalcInitialPosition(minfo, btPrice, 0, balance);
            if (minfo.Leverage == 0) balance -= pos * btPrice;
            btSize = pos;
        }

        trades.Add(new BTTrade(btTime, btPrice)
        {
            Size = btSize,
            Bal = balance,
            Pos = pos
        });

        try
        {
            double totalSpend = 0, pl = 0, btNormAccum = 0, btNormProfit = 0;
            foreach (var price in priceSource.Skip(1))
            {
                var minsize = Math.Max(minfo.MinSize, cfg.MinSize);
                if (Math.Abs(price.Price - btPrice) == 0) continue;

                var btEvent = BTEvent.NoEvent;
                var p = price.Price;

                var tk = new Ticker(p, p, p, price.Time);
                var prevBal = balance;
                var enableAlert = true;

                var eq = s.GetCenterPrice(btPrice, pos);
                double dir = p > eq ? -1 : 1;
                s.OnIdle(minfo, tk, pos, balance);
                var adjbal = Math.Max(balance, 0.0);
                var readOnlyOrder = s.GetNewOrder(minfo, btPrice * 0.9 + p * 0.1, p, dir, pos, adjbal, false);

                var oPrice = readOnlyOrder.Price;
                var oSize = readOnlyOrder.Size;
                var oAlert = readOnlyOrder.Alert;

                if (oPrice != 0)
                {
                    p = oPrice;
                }

                var dprice = p - btPrice;
                var pchange = pos * dprice;
                pl += pchange;
                if (minfo.Leverage != 0) balance += pchange;

                if (oSize != 0 && oSize * dir < 0)
                {
                    oSize = 0;
                }
                var orgsize = oSize;

                //TODO: https://github.com/ondra-novak/mmbot/blob/master/src/main/mtrader.cpp#L1588
                oSize = MarketInfo.AdjValue(oSize, minfo.AssetStep, MarketInfo.Rounded);
                if (cfg.MaxBalance.HasValue)
                {
                    if (pos > cfg.MaxBalance) oSize = 0;
                    else if (oSize + pos > cfg.MaxBalance) oSize = cfg.MaxBalance.Value - pos;
                }
                if (cfg.MinBalance.HasValue)
                {
                    if (pos < cfg.MinBalance) oSize = 0;
                    else if (oSize + pos < cfg.MinBalance) oSize = cfg.MinBalance.Value - pos;
                }
                if (minfo.Leverage != 0)
                {
                    var maxLev = cfg.MaxLeverage != 0 ? Math.Min(cfg.MaxLeverage, minfo.Leverage) : minfo.Leverage;
                    var maxAbsPos = (adjbal * maxLev) / btPrice;
                    var newPos = Math.Abs(pos + oSize);
                    var curPos = Math.Abs(pos);
                    if (newPos > curPos && newPos > maxAbsPos)
                    {
                        if (cfg.AcceptLoss != 0)
                        {
                            s.Reset();
                            s.OnIdle(minfo, tk, pos, adjbal);
                            btEvent = BTEvent.AcceptLoss;
                        }
                        else
                        {
                            btEvent = BTEvent.MarginCall;
                        }
                        oSize = 0;
                        orgsize = 0;
                    }
                }
                if (minfo.MinVolume != 0)
                {
                    var mvs = minfo.MinVolume / btPrice;
                    minsize = Math.Max(minsize, mvs);
                }
                if (oSize != 0 && Math.Abs(oSize) < minsize)
                {
                    if (Math.Abs(oSize) < minsize * 0.5)
                    {
                        oSize = 0;
                    }
                    else
                    {
                        oSize = Math.Sign(oSize) * minsize;
                    }
                }
                if (cfg.MaxSize != 0 && Math.Abs(oSize) > cfg.MaxSize)
                {
                    oSize = cfg.MaxSize * Math.Sign(oSize);
                }

                if (cfg.TradeWithinBudget && oSize * pos > 0 && s.CalcCurrencyAllocation(oSize) < 0)
                {
                    oSize = 0;
                    btEvent = BTEvent.NoBalance;
                }


                if (minfo.Leverage == 0)
                {
                    if (oSize + pos < 0)
                    {
                        oSize = -pos;
                        orgsize = oSize; //if zero - allow alert
                    }
                    var chg = oSize * p;
                    if (balance - chg < 0 || pos + oSize < -(Math.Abs(pos) + Math.Abs(oSize)) * 1e-10)
                    {
                        if (negBal)
                        {
                            btEvent = BTEvent.NoBalance;
                        }
                        else
                        {
                            if (cfg.AcceptLoss != 0)
                            {
                                s.Reset();
                                s.OnIdle(minfo, tk, pos, adjbal);
                                btEvent = BTEvent.AcceptLoss;
                            }
                            oSize = 0;
                            orgsize = 0; //allow alert this time
                            chg = 0;
                        }
                    }
                    balance -= chg;
                    pos += oSize;
                }
                else
                {
                    switch (balance)
                    {
                        case <= 0 when prevBal > 0:
                            btEvent = BTEvent.Liquidation;
                            oSize -= pos;
                            break;

                        case <= 0:
                            btEvent = BTEvent.NoBalance;
                            break;

                        default:
                            {
                                var mb = balance + dprice * (pos + oSize);
                                if (mb < 0)
                                {
                                    btEvent = BTEvent.MarginCall;
                                }

                                break;
                            }
                    }
                    if (oSize == 0 && cfg.MaxLeverage != 0 && cfg.ReduceOnLeverage)
                    {
                        var maxPos = adjbal / p;
                        if (maxPos < Math.Abs(pos))
                        {
                            maxPos *= Math.Sign(pos);
                            var diff = maxPos - pos;
                            oAlert = AlertType.Disabled;
                            // oPrice = p;
                            oSize = diff;
                        }
                    }
                    pos += oSize;
                }

                if (oSize == 0 && orgsize != 0 && oAlert != AlertType.Forced)
                {
                    enableAlert = false;
                }

                double btNeutralPrice = 0, btOpenPrice = 0;
                string btInfo;
                if (enableAlert)
                {
                    var (tres, _) = s.OnTrade(minfo, p, oSize, pos, balance);
                    btNeutralPrice = tres.NeutralPrice;
                    var normAccum = double.IsFinite(tres.NormAccum) ? tres.NormAccum : 0;
                    btNormAccum += normAccum;
                    btNormProfit += double.IsFinite(tres.NormProfit) ? tres.NormProfit : 0;
                    btOpenPrice = tres.OpenPrice;

                    if (oSize * (oSize - normAccum) > 1) oSize -= normAccum;
                    btInfo = s.DumpStatePretty(minfo);
                }
                else
                {
                    btInfo = JsonSerializer.Serialize(new Dictionary<string, double>
                {
                    { "Rejected size", orgsize},
                    { "Min size", minsize },
                    { "Direction", dir},
                    { "Equilibrium", eq},
                });
                }
                if (spend)
                {
                    var alloc = s.CalcCurrencyAllocation(p);
                    if (alloc > 0 && alloc < balance)
                    {
                        totalSpend += balance - alloc;
                        balance = alloc;
                    }
                }

                trades.Add(new BTTrade(price.Time, p, oSize)
                {
                    Pl = pl,
                    Pos = pos,
                    Bal = balance + totalSpend,
                    UnspendBalance = balance,
                    BtEvent = btEvent,
                    NeutralPrice = btNeutralPrice,
                    NormProfit = btNormProfit,
                    NormAccum = btNormAccum,
                    NormProfitTotal = btNormProfit + btNormAccum * p,
                    OpenPrice = btOpenPrice,
                    Info = btInfo
                });

                if (minfo.Leverage == 0) continue;

                var minbal = Math.Abs(pos) * p / (2 * minfo.Leverage);
                if (!(balance > minbal)) continue;

                var rbal1 = balance + pos * (price.Pmin - p);
                var rbal2 = balance + pos * (price.Pmax - p);
                var trig = false;
                if (rbal1 <= minbal)
                {
                    trig = true;
                    btPrice = price.Pmin;
                }
                else if (rbal2 <= minbal)
                {
                    trig = true;
                    btPrice = price.Pmax;
                }

                if (!trig) continue;

                var df = pos * (btPrice - p);
                pl += df;
                balance += df;

                btNormProfit = 0;
                btNormAccum = 0;
                btEvent = BTEvent.Liquidation;

                trades.Add(new BTTrade(price.Time, p, -pos)
                {
                    Pl = pl,
                    Pos = 0,
                    Bal = balance + totalSpend,
                    UnspendBalance = balance,
                    BtEvent = btEvent,
                    NeutralPrice = btNeutralPrice,
                    NormProfit = btNormProfit,
                    NormAccum = btNormAccum,
                    NormProfitTotal = 0,
                    OpenPrice = btOpenPrice,
                    Info = null
                });

            }
        }
        catch (Exception e)
        {
            if (!trades.Any()) throw;
            var t = trades[^1];
            trades[^1] = t with
            {
                Time = t.Time + 3600 * 1000,
                Info = JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    {"error", e.Message}
                })
            };
        }

        if (minfo.InvertPrice)
        {
            return trades.Select(x => x with
            {
                NeutralPrice = 1.0 / x.NeutralPrice,
                OpenPrice = 1.0 / x.OpenPrice,
                Pos = -x.Pos,
                Price = 1.0 / x.Price,
                Size = -x.Size
            });
        }

        return trades;
    }
}