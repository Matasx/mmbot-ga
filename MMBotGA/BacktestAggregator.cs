using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MMBotGA
{
    internal class BacktestAggregator : IBacktest
    {
        private readonly ICollection<IBacktest> _backtests;
        private readonly Func<IEnumerable<double>, double> _aggregationFunc;

        public BacktestAggregator(ICollection<IBacktest> backtests, Func<IEnumerable<double>, double> aggregationFunc = null)
        {
            _backtests = backtests;
            _aggregationFunc = aggregationFunc ?? (x => x.Min());
        }

        public async Task<double> TestAsync(BacktestRequest request)
        {
            if (_backtests.Count == 0) return 0;

            var results = new List<double>();            
            foreach (var backtest in _backtests)
            {
                results.Add(await backtest.TestAsync(request));
            }

            return _aggregationFunc(results);
        }
    }
}
