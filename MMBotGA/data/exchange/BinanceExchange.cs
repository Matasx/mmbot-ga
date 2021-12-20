namespace MMBotGA.data.exchange
{
    internal class BinanceExchange : IExchange
    {
        public string Name => "BINANCE";
        public string GetSymbol(Pair pair)
        {
            return $"{pair.Asset}{pair.Currency}".ToUpperInvariant();
        }
    }
}