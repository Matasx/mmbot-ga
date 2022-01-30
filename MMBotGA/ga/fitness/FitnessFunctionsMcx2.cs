using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using MMBotGA.backtest;
using MMBotGA.dto;

namespace MMBotGA.ga.fitness
{
    internal static class FitnessFunctionsMcx2
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FitnessFunctionsMcx));


        #region Nefunguje - Třebas opraviti
        public static double TightenNeutralPriceToLast(ICollection<RunResponse> results, double tightenNeutralPriceThreshold)
        {
            if (results.Count == 0) { return 0; }

            double deviation = 0;
            double deviatedTrades = 0;
            IEnumerable<RunResponse> resultsWithInfo = results.Where(x => x.Info != null).Where(x => x.Upnl < 0);
            double resultsWithInfoCounted = resultsWithInfo.Count();

            foreach (var resultWithInfo in resultsWithInfo)
            {
                double priceLast = resultWithInfo.Info.PriceLast;
                double priceNeutral = resultWithInfo.Info.PriceNeutral;
                if (priceLast != 0 && priceNeutral != 0)
                {
                    double numerator = Math.Abs(priceLast - priceNeutral);
                    double denominator = priceLast + priceNeutral / 2;

                    if (numerator != 0)
                    {
                        double percentageDiffCalculation = (numerator / denominator) * 100;
                        deviation = percentageDiffCalculation;

                        if (deviation > tightenNeutralPriceThreshold) { deviatedTrades += 1; }
                    }
                }
            }

            double deviatedTradesRatio = deviatedTrades / (double)results.Count();
            double deviationThresholdActual = 1 - deviatedTradesRatio;

            return deviationThresholdActual;
        }

        #endregion

        #region Outdated
        public static double TradeCountFactor(ICollection<RunResponse> results)
        {
            if (results.Count < 2) return 0;
            var last = results.Last();
            var first = results.First();

            var trades = results.Count(x => x.Sz != 0);
            var alerts = 1 - (results.Count - trades) / (double)results.Count;

            var days = (last.Tm - first.Tm) / 86400000d;
            var tradesPerDay = trades / days;

            const int mean = 15;
            const int delta = 5; // target trade range is 7 - 22 trades per day

            var x = Math.Abs(tradesPerDay - mean); // 0 - inf, 0 is best
            var y = Math.Max(x - delta, 0) + 1; // 1 - inf, 1 is best ... 
            var r = 1 / y;

            var alertsRatio = alerts / trades;

            //Pokud mám více jak 3% alertů v celém spektru, špatný backtest.
            if (alertsRatio > 0.03) { return 0; } else { return r * alerts; }



            //return Normalize(trades, 1000, 3000, null) * alerts;
        }
        public static double IncomePerDayRatio(ICollection<RunResponse> results)
        {
            if (results.Count < 2)
            {
                return 0;
            }

            var firstResult = results.First();
            var lastResult = results.Last();

            double totalDays = (lastResult.Tm - firstResult.Tm) / 86400000;

            if (totalDays <= 0)
            {
                return 0;
            }

            var backtestStartingPoint = firstResult.Tm;

            var goodDay = 0;
            for (var day = 0; day < totalDays; day++)
            {
                var firstChunkTrade = backtestStartingPoint + day * 86400000;
                var lastChunkTrade = backtestStartingPoint + (day + 1) * 86400000;

                var dayTrades = results
                    .Where(x => x.Tm >= firstChunkTrade && x.Tm < lastChunkTrade)
                    .ToList();

                if (dayTrades.Any())
                {
                    var np = dayTrades.Last().Np - dayTrades.First().Np;
                    var pl = dayTrades.Last().Pl - dayTrades.First().Pl;

                    if (pl > 0 && np > 0)
                    {
                        goodDay++;
                    }
                }
            }

            var goodDayRatio = (goodDay / totalDays) + 0.5;
            // tie closer.
            return goodDayRatio;
        }
        public static double NormalizedProfitPerYear(BacktestRequest request, ICollection<RunResponse> results)
        {
            // npc: https://github.com/ondra-novak/mmbot/blob/141f74206f7b1938fa0903d20486f4962293ad1e/www/admin/code.js#L1872

            if (results.Count < 2) return 0;
            var last = results.Last();
            var first = results.First();

            var interval = last.Tm - first.Tm;
            var profit = Math.Max(last.Npl * 31536000000 / (interval * request.RunRequest.Balance), 0);

            if (profit == 0) return 0;

            return Normalize(profit, 0.3, 0.7, null);
        }
        public static double PnlProfitPerYear(BacktestRequest request, ICollection<RunResponse> results)
        {
            // pc: https://github.com/ondra-novak/mmbot/blob/141f74206f7b1938fa0903d20486f4962293ad1e/www/admin/code.js#L1873

            if (results.Count < 2) return 0;
            var last = results.Last();
            var first = results.First();

            var interval = last.Tm - first.Tm;
            var profit = Math.Max(last.Pl * 31536000000 / (interval * request.RunRequest.Balance), 0);

            if (profit == 0) return 0;

            return Normalize(profit, 0.3, 0.7, null);
        }
        public static double MaxCost(BacktestRequest request, ICollection<RunResponse> results)
        {
            double cost = 0;
            double maxCost = 0;

            foreach (var trade in results)
            {
                cost = cost + (trade.Sz * trade.Pr);
                if (cost > maxCost) { maxCost = cost; }
            }

            var balance = request.RunRequest.Balance;
            var budgetRatioInverse = 1 - (maxCost / balance);

            return budgetRatioInverse;
        }
        public static double LowerPositionOverall(BacktestRequest request, ICollection<RunResponse> results, double balancePercentage)
        {
            if (results.Count < 1) return 0;

            var balanceEval = balancePercentage * request.RunRequest.Balance;

            //All trades with position above x% of balance
            var tradesHighPosition = results.Count(x => x.Pr * x.Ps > balanceEval);

            var lowPosOverall = 1 - (double)tradesHighPosition / (double)results.Count();

            return lowPosOverall;
        }
        #endregion

        public static double CheckForEvents(ICollection<RunResponse> results)
        {
            if (results.Where(x => x.Event != null).Where(x => x.Event == "margin_call").Count() > 0)
            {
                return 0;
            }
            else
            {
                return 1;
            };
        }

        public static double TightenNplRpnl(ICollection<RunResponse> results, double tightenNplRpnlThreshold)
        {
            if (results.Count == 0) { return 0; }

            double deviatedTrades = 0;
            double resultsCounted = results.Count();

            foreach (var result in results)
            {
                double npl = result.Npl;
                double rpnl = result.Rpnl;

                double numerator = Math.Abs(npl - rpnl);
                double denominator = npl + rpnl / 2;

                if (numerator != 0)
                {
                    double percentageDiffCalculation = (numerator / denominator) * 100;
                    if (percentageDiffCalculation > tightenNplRpnlThreshold) { deviatedTrades += 1; }
                }
            }

            double deviatedTradesRatio = deviatedTrades / resultsCounted;
            double deviationThresholdActual = 1 - deviatedTradesRatio;
            return deviationThresholdActual;
        }

        public static double Rrr(ICollection<RunResponse> results)
        {
            if (results.Count < 1) return 0;

            double maxPl = 0, minPl = 0, maxDowndraw = 0;
            foreach (var trade in results)
            {
                if (trade.Pl > maxPl)
                {
                    minPl = maxPl = trade.Pl;
                }

                if (trade.Pl < minPl)
                {
                    minPl = trade.Pl;
                    var downdraw = maxPl - minPl;
                    if (downdraw > maxDowndraw)
                    {
                        maxDowndraw = downdraw;
                    }
                }
            }

            var result = Math.Max(maxPl / maxDowndraw, 0);
            return Normalize(result, 5, 30, null);
        }

        public static FitnessComposition NpaRRR(BacktestRequest request, ICollection<RunResponse> results)
        {
            if (results == null || results.Count == 0) return new FitnessComposition();

            //Jsem mnich a zisk mne nezajímá.
            //Některé páry oscilují tak debilně, že snažit se je donutit k zisku, je kontraproduktivní
            //MartinGale nedokáže efektivně mitigovat pump&dump coiny, proto GA vyrábí kompromis
            //aby se dostala na požadovaný zisk. 
            //Ziskovost vychází z cenové oscilace daného páru. 

            #region Outdated
            const double nppyWeight = 0.00;
            const double pppyWeight = 0.00;
            const double ipdrWeight = 0.00;
            const double lpoWeight = 0.00;
            const double maxCostWeight = 0.00;
            const double tightenNeutralPriceWeight = 0.00; //Nefunguje moc dobře, nahrazeno NplRpnl. (bylo by zajímavé toto přezkoumat).
            //Balance nad 10% reportuj jako vysokou pozici.
            const double balanceThreshold = 0.1;
            //Nad 2% deviace od neutrální ceny snižuj Fitness.
            const double tightenNeutralPriceThreshold = 2; //Nefunguje při splitnutém grafu ! (Malém jsem se tady při ladění posral...)
            const double tradeCountWeight = 0.00;
            #endregion

            const double rrrWeight = 0.20;
            const double tightenNplRpnlWeight = 0.80;

            //tightenNplRpnl je proměnlivý údaj a velmi záleží na páru
            //chtělo by to matematickou rovnici která by určila optimální NplRpnl bez toho, aniž by overfitnul.
            //Napadla mne matice která by dle oscilace páru určila tento parametr. 
            //Je navázáno na Exponent u Gamma funkcí.
            const double tightenNplRpnlThreshold = 1.5; // % oscilace profit&loss kolem normalized profit.

            //Debug
            //Debug.Assert(Math.Abs(nppyWeight + pppyWeight + ipdrWeight + lpoWeight + rrrWeight + tradeCountWeight + maxCostWeight + tightenNeutralPriceWeight + tightenNplRpnlWeight - 1) < 0.01);

            var eventCheck = CheckForEvents(results); //0-1, nic jiného nevrací.
            var result = new FitnessComposition();

            result.Fitness = (rrrWeight * (result.RRR = Rrr(results))
              //+ tradeCountWeight * (result.TradeCountFactor = TradeCountFactor(results))
              + tightenNplRpnlWeight * (result.TightenNplRpnl = TightenNplRpnl(results, tightenNplRpnlThreshold)))
              * eventCheck;

            #region Outdated
            //result.Fitness = (nppyWeight * (result.NpProfitPerYear = NormalizedProfitPerYear(request, results))
            //  + pppyWeight * (result.PnlProfitPerYear = PnlProfitPerYear(request, results))
            //  + ipdrWeight * (result.IncomePerDayRatio = IncomePerDayRatio(results))
            //  + rrrWeight * (result.RRR = Rrr(results))
            //  + tradeCountWeight * (result.TradeCountFactor = TradeCountFactor(results))
            //  + lpoWeight * (result.LowerPositionFactor = LowerPositionOverall(request, results, balanceThreshold))
            //  + maxCostWeight * (result.MaxCostFactor = MaxCost(request, results))
            //  + tightenNeutralPriceWeight * (result.TightenNeutralPriceToLast = TightenNeutralPriceToLast(results, tightenNeutralPriceThreshold)))
            //  + tightenNplRpnlWeight * (result.TightenNplRpnl = TightenNplRpnl(results, tightenNplRpnlThreshold))
            //  * eventCheck;
            //var fitness = (nppyEval + pppyEval + ipdrEval + rrrEval + tradeCountEval + lowerPosEval + maxCostEval + minMaxBalanceTheBalanceEval) * eventCheck;
            //Formát výpisu zachovat, čárka a mezera se používají v LogAnalyzer.ps1 dle které se splitují hodnoty !
            //Log.Info($"Fitness : {fitness}, nppyEval : {nppyEval}, pppyEval : {pppyEval}, ipdrEval : {ipdrEval}, rrrEval : {rrrEval}, tradeCountEval : {tradeCountEval}, lowerPosEval : {lowerPosEval}, MaxCostEval : {maxCostEval}, EventCheck : {eventCheck}");
            #endregion

            return result;
        }

        public static double Normalize(double value, double target, double virtualMax, double? cap)
        {
            if (value <= 0) return 0;
            var capped = Math.Min(value, cap ?? value);
            var baseline = Math.Min(capped, target) / target;
            var aboveTarget = Math.Max(0, value - target);
            var vMaxAboveTarget = virtualMax - target;
            var extra = Math.Min(aboveTarget, vMaxAboveTarget) / vMaxAboveTarget;

            return 0.75 * baseline + 0.25 * extra;
        }
    }
}