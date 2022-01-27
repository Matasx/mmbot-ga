using MMBotGA.backtest;

namespace MMBotGA.ga.abstraction
{
    internal class Batch
    {
        public string Name { get; }
        public BacktestData[] BacktestData { get; }
        public ICustomChromosome AdamChromosome { get; }

        public Batch(string name, ICustomChromosome adamChromosome, BacktestData[] backtestData)
        {
            Name = name;
            BacktestData = backtestData;
            AdamChromosome = adamChromosome;
        }
    }
}