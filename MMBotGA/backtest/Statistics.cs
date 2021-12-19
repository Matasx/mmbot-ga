namespace MMBotGA.backtest
{
    internal class Statistics
    {
        public int TradeCount { get; set; }
        public double Balance { get; set; }

        public double PlAbs { get; set; }
        public double PlYrAbs { get; set; }
        public double PlPerc { get; set; }

        public double NormPlAbs { get; set; }
        public double NormPlYrAbs { get; set; }
        public double NormPlPerc { get; set; }

        public double MaxPosAbs { get; set; }
        public double MaxCostAbs { get; set; }
        public double MaxProfitAbs { get; set; }
        public double MaxLossAbs { get; set; }
        public double MaxLossPerc { get; set; }

        public double RRR { get; set; }
    }
}