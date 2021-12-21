using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMBotGA.api;
using MMBotGA.dto;
using MMBotGA.ga.fitness;
using MMBotGA.io;

namespace MMBotGA.backtest
{
    internal class Backtest : IBacktest<ICollection<RunResponse>>
    {
        private readonly SemaphoreSlim _semaphore = new(1);
        private readonly ApiLease _api;
        private readonly BacktestData _data;
        private readonly Func<BacktestRequest, ICollection<RunResponse>, double> _fitnessEvaluator;
        private readonly IDictionary<Api, Context> _contexts = new Dictionary<Api, Context>();

        private Minfo _minfo;

        public Backtest(ApiLease api, BacktestData data, Func<BacktestRequest, ICollection<RunResponse>, double> fitnessEvaluator = null)
        {
            _api = api;
            _data = data;
            _fitnessEvaluator = fitnessEvaluator ?? FitnessFunctionsMcx.NpaRrr;
        }

        public async Task<BacktestResult<ICollection<RunResponse>>> TestAsync(BacktestRequest request)
        {
            var api = await _api.LeaseAsync();
            try
            {
                await _semaphore.WaitAsync();
                Context context;
                try
                {
                    if (!_contexts.TryGetValue(api, out context))
                    {
                        _contexts[api] = context = new Context(this, api);
                    }

                    _minfo ??= await context.GetMinfoAsync();
                }
                finally
                {
                    _semaphore.Release();
                }
                request.RunRequest.Minfo = _minfo;
                request.RunRequest.Balance = _data.Balance;
                return await context.TestAsync(request);
            }
            finally
            {
                _api.EndLease(api);
            }
        }

        private class Context
        {
            private readonly Backtest _backtest;
            private readonly Api _api;

            private FileIdResponse _dataset;
            private readonly SemaphoreSlim _semaphore = new(1);

            public Context(Backtest backtest, Api api)
            {
                _backtest = backtest;
                _api = api;
            }

            private async Task InitAsync()
            {
                var data = await CsvLoader.LoadAsync(_backtest._data.SourceFile, _backtest._data.Reverse);
                _dataset = await _api.UploadAsync(data);
            }

            private async Task CheckInitAsync()
            {
                for (var i = 0; i < 3; i++)
                {
                    try
                    {
                        await _semaphore.WaitAsync();
                        if (_dataset == null)
                        {
                            await InitAsync();
                        }
                        return;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        await Task.Delay(5000);
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }
            }

            public async Task<Minfo> GetMinfoAsync()
            {
                Exception exception = null;
                for (var i = 0; i < 3; i++)
                {
                    try
                    {
                        return await _api.GetInfoAsync(_backtest._data.Broker, _backtest._data.Pair);
                    }
                    catch (Exception e)
                    {
                        exception = e;
                        Console.WriteLine(e);
                        await Task.Delay(5000);
                    }
                }

                throw exception;
            }

            public async Task<BacktestResult<ICollection<RunResponse>>> TestAsync(BacktestRequest request)
            {
                for (var i = 0; i < 3; i++)
                {
                    try
                    {
                        await CheckInitAsync();
                        return await DoEvaluateAsync(request);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        await _semaphore.WaitAsync();
                        try
                        {
                            _dataset = null;
                        }
                        catch (Exception ee)
                        {
                            Console.WriteLine(ee);
                        }
                        finally
                        {
                            _semaphore.Release();
                        }
                        await Task.Delay(5000);
                    }
                }

                return new BacktestResult<ICollection<RunResponse>>(default, default);
            }

            private async Task<BacktestResult<ICollection<RunResponse>>> DoEvaluateAsync(BacktestRequest request)
            {
                request.GenTradesRequest.Source = _dataset.Id;
                var trades = await _api.GenerateTradesAsync(request.GenTradesRequest);

                request.RunRequest.Source = trades.Id;
                var response = await _api.RunAsync(request.RunRequest);

                return new BacktestResult<ICollection<RunResponse>>(_backtest._fitnessEvaluator(request, response), response);
            }
        }
    }
}
