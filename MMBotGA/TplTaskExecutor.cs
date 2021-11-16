using GeneticSharp.Infrastructure.Framework.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MMBotGA
{
    public class TplTaskExecutor : ParallelTaskExecutor
    {
        private readonly int _maxDegreeOfParallelism;

        public TplTaskExecutor(int maxDegreeOfParallelism)
        {
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        public override bool Start()
        {
            try
            {
                var startTime = DateTime.Now;
                CancellationTokenSource = new CancellationTokenSource();
                var result = new ParallelLoopResult();

                try
                {
                    var options = new ParallelOptions()
                    {
                        CancellationToken = CancellationTokenSource.Token,
                        MaxDegreeOfParallelism = _maxDegreeOfParallelism
                    };

                    result = Parallel.ForEach(Tasks, options, (task, state, i) =>
                    {
                        // Check if any has called Break().
                        if (state.ShouldExitCurrentIteration && state.LowestBreakIteration < i)
                            return;

                        // Execute the target function (fitness).
                        task();

                        // If cancellation token was requested OR take more time expected on Timeout property, 
                        // then stop the running.
                        if ((CancellationTokenSource.IsCancellationRequested && !state.ShouldExitCurrentIteration)
                        || ((DateTime.Now - startTime) > Timeout && !state.ShouldExitCurrentIteration))
                            state.Break();
                    });
                }
                catch (OperationCanceledException)
                {
                    // Mute cancellation exception.
                }

                return result.IsCompleted;
            }
            finally
            {
                IsRunning = false;
            }
        }
    }
}
