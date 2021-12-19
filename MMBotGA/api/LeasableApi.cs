using System.Threading;
using System.Threading.Tasks;

namespace MMBotGA.api
{
    internal class LeasableApi
    {
        private readonly SemaphoreSlim _semaphore;
        public int Available => _semaphore.CurrentCount;
        public Api Api { get; private set; }

        public LeasableApi(int leaseCount, Api api)
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
