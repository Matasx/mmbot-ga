internal class StreamStdev
{
    public StreamStdev(int interval)
    {
        _sum = new StreamSum(interval);
    }

    //feed value and return result
    public double Add(double v)
    {
        var s = _sum.Add(v * v);
        return Math.Sqrt(s / _sum.Size());
    }

    public int Size() => _sum.Size();

    private readonly StreamSum _sum;
}