using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MMBotGA.api
{
    internal class ApiLease
    {
        private readonly SemaphoreSlim _semaphore = new(1);
        private readonly IDictionary<Api, LeasableApi> _apis;
        public int Available => _apis.Values.Sum(x => x.Available);

        public ApiLease(params LeasableApi[] apis)
        {
            _apis = apis.ToDictionary(x => x.Api, x => x);
        }

        public async Task<Api> LeaseAsync()
        {
            try
            {
                await _semaphore.WaitAsync();
                LeasableApi api;
                while ((api = _apis.Values.FirstOrDefault(x => x.Available > 0)) == null)
                {
                    await Task.Delay(100);
                }
                await api.LeaseAsync();
                return api.Api;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void EndLease(Api api)
        {
            _apis[api].EndLease();
        }
    }
}
