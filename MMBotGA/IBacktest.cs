using System.Threading.Tasks;

namespace MMBotGA
{
    internal interface IBacktest
    {
        Task<double> TestAsync(BacktestRequest request);
    }
}
