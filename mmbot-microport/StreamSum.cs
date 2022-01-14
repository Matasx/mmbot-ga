internal class StreamSum
{
    public StreamSum(int interval)
    {
        _interval = interval;
        _n = new Queue<double>(interval + 1);
    }

    public double Add(double v)
    {
        _sum += v;
        _n.Enqueue(v);
        if (_n.Count > _interval)
        {
            _sum -= _n.Dequeue();
        }
        return _sum;
    }

    public int Size() => _n.Count;
    
    private readonly int _interval;
    private readonly Queue<double> _n;
    private double _sum;
}