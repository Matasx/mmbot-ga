using System;

namespace MMBotGA.backtest
{
    internal struct BacktestData
    {
        public string SourceFile { get; set; }
        public bool Reverse { get; set; }
        public string Broker { get; set; }
        public string Pair { get; set; }
        public double Balance { get; set; }
        public DateTime? Start { get; set; }
        public int? Offset { get; set; }
        public int? Limit { get; set; }
    }
}
