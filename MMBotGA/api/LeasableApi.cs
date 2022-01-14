using System.Threading;
using System.Threading.Tasks;
using MMBot.Api;

namespace MMBotGA.api
{
    internal class LeasableApi
    {
        private readonly SemaphoreSlim _semaphore;
        public int Available => _semaphore.CurrentCount;
        public IMMBotApi Api { get; }

        public LeasableApi(int leaseCount, IMMBotApi api)
        {
            _semaphore = new SemaphoreSlim(leaseCount);
            Api = api;
        }

        public async Task LeaseAsync()
        {
            await _semaphore.WaitAsync();
        }

        public void EndLease()
        {
            _semaphore.Release();
        }
    }
}
