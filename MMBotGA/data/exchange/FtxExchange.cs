namespace MMBotGA.data.exchange
{
    internal class FtxExchange : IExchange
    {
        public string Name => "FTX";
        public string GetSymbol(Pair pair)
        {
            return $"{pair.Asset}-{pair.Currency}".ToUpperInvariant();
        }
    }
}