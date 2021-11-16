namespace MMBotGA
{
    internal struct BacktestData
    {
        public string SourceFile { get; set; }
        public bool Reverse { get; set; }
        public string Broker { get; set; }
        public string Pair { get; set; }
        public double Balance { get; set; }
    }
}
