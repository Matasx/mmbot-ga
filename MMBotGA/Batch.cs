namespace MMBotGA;

internal class Batch
{
    public string Name { get; }
    public BacktestData[] BacktestData { get; }

    public Batch(string name, BacktestData[] backtestData)
    {
        Name = name;
        BacktestData = backtestData;
    }
}