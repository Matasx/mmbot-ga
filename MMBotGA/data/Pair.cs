namespace MMBotGA.data
{
    internal class Pair
    {
        public string Asset { get; set; }
        public string Currency { get; set; }

        public Pair(string asset, string currency)
        {
            Asset = asset;
            Currency = currency;
        }
    }
}