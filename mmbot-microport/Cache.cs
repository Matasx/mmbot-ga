public class Cache<TKey, TValue>
{
    private readonly Dictionary<TKey, CacheEntry> _data = new();
    private readonly int _capacity;
    private readonly object _lock = new();
    private DateTime _lastCheck = DateTime.Now;

    public Cache(int capacity)
    {
        _capacity = capacity;
    }

    public (bool Success, TValue Value) Get(TKey key)
    {
        lock (_lock)
        {
            if (!_data.TryGetValue(key, out var value))
            {
                return new ValueTuple<bool, TValue>(false, default);
            }

            value.Touch();
            return new ValueTuple<bool, TValue>(true, value.Value);
        }
    }

    public void Set(TKey key, TValue value)
    {
        lock (_lock)
        {
            if (_data.Count >= _capacity && (DateTime.Now - _lastCheck).TotalMinutes > 1)
            {
                var remove = _data
                    .Where(x => !x.Value.Valid)
                    .Select(x => x.Key)
                    .ToArray();

                foreach (var key1 in remove)
                {
                    _data.Remove(key1);
                }
                _lastCheck = DateTime.Now;
            }

            _data[key] = new CacheEntry(value);
        }
    }

    private class CacheEntry
    {
        private DateTime _lastAccess;
        public TValue Value { get; }
        public bool Valid => (DateTime.Now - _lastAccess).TotalMinutes < 2;

        public CacheEntry(TValue value)
        {
            _lastAccess = DateTime.Now;
            Value = value;
        }

        public void Touch()
        {
            _lastAccess = DateTime.Now;
        }
    }
}