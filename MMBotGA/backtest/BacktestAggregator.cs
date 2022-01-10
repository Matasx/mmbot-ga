using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MMBotGA.backtest
{
    internal class BacktestAggregator<TData> : IBacktest<TData>
    {
        private readonly ICollection<IBacktest<TData>> _backtests;
        private readonly Func<IEnumerable<BacktestResult<TData>>, BacktestResult<TData>> _aggregationFunc;

        public BacktestAggregator(ICollection<IBacktest<TData>> backtests, Func<IEnumerable<BacktestResult<TData>>, BacktestResult<TData>> aggregationFunc = null)
        {
            _backtests = backtests;
            _aggregationFunc = aggregationFunc ?? DefaultAggregationFunc;
        }

        private static BacktestResult<TData> DefaultAggregationFunc(IEnumerable<BacktestResult<TData>> backtestResults)
        {
            return backtestResults.OrderBy(x => x.Fitness.Fitness).FirstOrDefault() ??
                   new BacktestResult<TData>(default, default);
        }

        public async Task<BacktestResult<TData>> TestAsync(BacktestRequest request)
        {
            if (_backtests.Count == 0) return new BacktestResult<TData>(default, default);

            var results = new List<BacktestResult<TData>>();            
            foreach (var backtest in _backtests)
            {
                results.Add(await backtest.TestAsync(request));
            }

            return _aggregationFunc(results);
        }
    }
}
