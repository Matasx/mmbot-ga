using System.Threading.Tasks;

namespace MMBotGA.backtest
{
    internal interface IBacktest<TData>
    {
        Task<BacktestResult<TData>> TestAsync(BacktestRequest request);
    }
}
