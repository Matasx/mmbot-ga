using System.Threading.Tasks;

namespace MMBotGA
{
    internal interface IBacktest<TData>
    {
        Task<BacktestResult<TData>> TestAsync(BacktestRequest request);
    }
}
