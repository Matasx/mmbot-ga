using System;

namespace MMBotGA.backtest
{
    internal static class BacktestStats
    {
        const int _frame = 5;
        private static readonly object _lock = new();
        private static long _count = 0;
        private static readonly Stat[] _stats = new Stat[_frame];
        private static Stat _lastFrame;
        public static double Rate
        {
            get
            {
                lock (_lock)
                {
                    return (double)(_count - _lastFrame?.Count) / (_frame - 1);
                }
            }
        }

        static BacktestStats()
        {
            for (int i = 0; i < _stats.Length; i++)
            {
                _stats[i] = new Stat();
            }
        }

        public static void Add()
        {
            lock(_lock)
            {
                var second = DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
                _count += (_lastFrame = _stats[second % _frame]).Add(second);
            }
        }

        private class Stat
        {
            private long _tick;
            public long Count { get; private set; }

            public long Add(long tick)
            {
                if (_tick != tick)
                {
                    var ret = Count - 1;
                    _tick = tick;
                    Count = 1;
                    return -ret;
                }

                Count++;
                return 1;
            }
        }
    }
}
