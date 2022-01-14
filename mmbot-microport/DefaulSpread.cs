internal class DefaulSpread : ISpreadFunction
{
    private class State
    {
        public readonly StreamSma Sma;
        public readonly StreamStdev Stdev;
        //public StreamBest<double, std::greater<double>> maxSpread;

        public State(int smaInterval, int stdevInterval)
        {
            Sma = new StreamSma(smaInterval);
            Stdev = new StreamStdev(stdevInterval);
        }
    }

    public DefaulSpread(double sma, double stdev, double forceSpread)
    {
        _sma = sma;
        _stdev = stdev;
        _forceSpread = forceSpread;
    }

    public object Start()
    {
        return new State((int)Math.Max(30, _sma * 60), (int)Math.Max(30, _stdev * 60));
    }

    public ISpreadFunction.Result Point(object state, double y)
    {
        var st = (State)state;

        var avg = st.Sma.Add(y);
        if (_forceSpread != 0)
        {
            return new ISpreadFunction.Result(true, _forceSpread, avg, 0);
        }

        var dv = st.Stdev.Add(y - avg);
        return new ISpreadFunction.Result(true, Math.Log((avg + dv) / avg), avg, 0);
    }

    private double _sma;
    private double _stdev;
    private double _forceSpread;
}