using MMBotGA.backtest;
using MMBotGA.dto;

namespace MMBotGA.ga
{
    internal static class ChromosomeExtensions
    {
        public static BacktestRequest ToBacktestRequest(this SpreadChromosome chromosome, Strategy strategy)
        {
            const bool sliding = false;
            var freeze = chromosome.Freeze;
            var dynMult = chromosome.DynMult;

            return new BacktestRequest
            {
                GenTradesRequest = new GenTradesRequest
                {
                    Sma = chromosome.Sma,
                    Stdev = chromosome.Stdev,
                    ForceSpread = 0,
                    Mult = chromosome.Mult,
                    Raise = chromosome.Raise,
                    Cap = chromosome.Cap,
                    Fall = chromosome.Fall,
                    Mode = chromosome.Mode,
                    Sliding = sliding,
                    SpreadFreeze = freeze,
                    DynMult = dynMult,
                    Reverse = false,
                    Invert = false,
                    Ifutures = false,
                    // Order2 = 50
                },
                RunRequest = new RunRequest
                {
                    // StartDate = "1.9.2021",
                    FillAtprice = true,
                    Reverse = false,
                    Invert = false,
                    NegBal = false,
                    InitPrice = 0,
                    Spend = false, // true applies to keep-value only
                    Config = new Config
                    {
                        Strategy = strategy,
                        Enabled = true,
                        AdjTimeout = 5,

                        SpreadCalcSmaHours = chromosome.Sma,
                        SpreadCalcStdevHours = chromosome.Stdev,
                        DynmultMode = chromosome.Mode,
                        DynmultSliding = sliding,
                        SpreadFreeze = freeze,
                        DynmultMult = dynMult,
                        DynmultRaise = chromosome.Raise,
                        DynmultFall = chromosome.Fall,
                        DynmultCap = chromosome.Cap,
                        SellStepMult = chromosome.Mult,
                        BuyStepMult = chromosome.Mult,
                        //SecondaryOrder = 50,
                    }
                }
            };
        }

        public static BacktestRequest ToBacktestRequest(this StrategyChromosome chromosome)
        {
            return chromosome.ToBacktestRequest(new Strategy
            {
                Type = "gamma",
                Function = chromosome.Function,
                Trend = chromosome.Trend,
                Reinvest = false,

                Exponent = chromosome.Exponent,
                Rebalance = chromosome.Rebalance.ToString()
            });
        }
    }
}
