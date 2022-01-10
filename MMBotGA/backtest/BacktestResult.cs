using MMBotGA.ga.fitness;

namespace MMBotGA.backtest
{
    internal class BacktestResult<T>
    {
        public FitnessComposition Fitness { get; }
        public T Data { get; }

        public BacktestResult(FitnessComposition fitness, T data)
        {
            Fitness = fitness;
            Data = data;
        }
    }
}