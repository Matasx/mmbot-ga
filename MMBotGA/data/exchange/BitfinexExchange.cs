using System;

namespace MMBotGA.data.exchange
{
    internal class BitfinexExchange : IExchange
    {
        public string Name => "BITFINEX";

        public string GetSymbol(Pair pair)
        {
            return "t" + $"{pair.Asset}{pair.Currency}".ToUpperInvariant();
        }

        public string GetRobotSymbol(Pair pair)
        {
            return Uri.EscapeDataString($"{pair.Asset}/{pair.Currency}".ToUpperInvariant());
        }
    }
}