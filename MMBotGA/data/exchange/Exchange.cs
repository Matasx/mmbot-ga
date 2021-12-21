namespace MMBotGA.data.exchange
{
    internal static class Exchange
    {
        public static IExchange Binance = new BinanceExchange();
        public static IExchange Kucoin = new KucoinExchange();
        public static IExchange Ftx = new FtxExchange();
        public static IExchange Bitfinex = new BitfinexExchange();
    }
}