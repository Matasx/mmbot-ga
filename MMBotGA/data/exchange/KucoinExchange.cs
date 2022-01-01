namespace MMBotGA.data.exchange
{
    internal class KucoinExchange : IExchange
    {
        public string Name => "KUCOIN";

        public string GetSymbol(Pair pair)
        {
            return $"{pair.Asset}-{pair.Currency}".ToUpperInvariant();
        }

        public string GetRobotSymbol(Pair pair)
        {
            return GetSymbol(pair);
        }
    }
}