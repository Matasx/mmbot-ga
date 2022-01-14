using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using MMBot.Api;
using MMBot.Api.dto;
using mmbot_microport.utils;
using MMBotGA.api;
using MMBotGA.ga.fitness;
using MMBotGA.io;

namespace MMBotGA.backtest
{
    internal class Backtest : IBacktest<ICollection<RunResponse>>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LoggingHandler));

        private readonly SemaphoreSlim _semaphore = new(1);
        private readonly ApiLease _api;
        private readonly BacktestData _data;
        private readonly Func<BacktestRequest, ICollection<RunResponse>, FitnessComposition> _fitnessEvaluator;
        private readonly IDictionary<IMMBotApi, Context> _contexts = new Dictionary<IMMBotApi, Context>();

        private Minfo _minfo;

        public Backtest(ApiLease api, BacktestData data, Func<BacktestRequest, ICollection<RunResponse>, FitnessComposition> fitnessEvaluator = null)
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
                request.GenTradesRequest.Limit = _data.Limit;
                request.GenTradesRequest.Offset = _data.Offset;
                if (_data.Start.HasValue)
                {
                    request.GenTradesRequest.BeginTime = UnixEpoch.GetEpochMs(_data.Start.Value);
                }

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
            private readonly IMMBotApi _api;

            private FileIdResponse _dataset;
            private readonly SemaphoreSlim _semaphore = new(1);

            public Context(Backtest backtest, IMMBotApi api)
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
                        Log.Error($"Exception while initializing data. Will retry ({2 - i}).", e);
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
                        Log.Error($"Exception while getting exchange info. Will retry ({2 - i}).", e);
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
                        Log.Error($"Exception while backtesting. Will retry ({2 - i}).", e);
                        await _semaphore.WaitAsync();
                        _dataset = null;
                        _semaphore.Release();
                        await Task.Delay(5000);
                    }
                }

                return new BacktestResult<ICollection<RunResponse>>(default, new List<RunResponse>());
            }

            private async Task<BacktestResult<ICollection<RunResponse>>> DoEvaluateAsync(BacktestRequest request)
            {
                request.GenTradesRequest.Source = _dataset.Id;
                var trades = await _api.GenerateTradesAsync(request.GenTradesRequest);

                request.RunRequest.Source = trades.Id;
                var response = await _api.RunAsync(request.RunRequest) ?? new List<RunResponse>();

                return new BacktestResult<ICollection<RunResponse>>(_backtest._fitnessEvaluator(request, response), response);
            }
        }
    }
}
