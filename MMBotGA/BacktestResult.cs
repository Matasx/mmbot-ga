namespace MMBotGA;

internal class BacktestResult<T>
{
    public double Fitness { get; }
    public T Data { get; }

    public BacktestResult(double fitness, T data)
    {
        Fitness = fitness;
        Data = data;
    }
}