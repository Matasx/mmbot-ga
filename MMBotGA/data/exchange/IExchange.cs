namespace MMBotGA.data.exchange
{
    internal interface IExchange
    {
        string Name { get; }

        string GetSymbol(Pair pair);

        string GetRobotSymbol(Pair pair);
    }
}