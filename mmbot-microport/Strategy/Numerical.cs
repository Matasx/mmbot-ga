namespace mmbot_microport.Strategy;

internal static class Numerical
{
    private const double Accuracy = 1e-6;

    public static double NumericSearchR1(double middle, Func<double, double> fn)
    {
        double min = 0;
        var max = middle;
        var reff = fn(middle);
        if (reff is 0 or double.NaN) return middle;
        var md = (min + max) / 2;
        var cnt = 1000;
        while ((max - min) / md > Accuracy && --cnt > 0)
        {
            var v = fn(md);
            if (double.IsNaN(v)) break;
            var ml = v * reff;
            if (ml > 0) max = md;
            else if (ml < 0) min = md;
            else return md;
            md = (min + max) / 2;
        }
        return md;
    }

    public static double NumericSearchR2(double middle, Func<double, double> fn)
    {
        double min = 0;
        var max = 1.0 / middle;
        var reff = fn(middle);
        if (reff is 0 or double.NaN) return middle;
        var md = (min + max) / 2;
        var cnt = 1000;
        while (md * (1.0 / min - 1.0 / max) > Accuracy && --cnt > 0)
        {
            var v = fn(1.0 / md);
            if (double.IsNaN(v)) break;
            var ml = v * reff;
            if (ml > 0) max = md;
            else if (ml < 0) min = md;
            else return 1.0 / md;
            md = (min + max) / 2;
        }
        return 1.0 / md;
    }

    private static double GenerateIntTable2(Func<double, double> fn, double a, double b, double fa, double fb, double error, double y, int lev, Action<double, double> outFn)
    {
        var w = b - a;
        var pa = w * fa;
        var pb = w * fb;
        var e = Math.Abs(pa - pb);
        if (!(e > error) || lev >= 16) return (pa + pb) * 0.5;
        
        var m = (a + b) * 0.5;
        var fm = fn(m);
        var sa = GenerateIntTable2(fn, a, m, fa, fm, error, y, lev + 1, outFn);
        y += sa;
        outFn(m, y);
        var sb = GenerateIntTable2(fn, m, b, fm, fb, error, y, lev + 1, outFn);
        return sa + sb;
    }

    public static void GenerateIntTable(Func<double, double> fn, double a, double b, double error, double y, Action<double, double> outFn)
    {
        outFn(a, y);
        var fa = fn(a);
        var fb = fn(b);
        var r = GenerateIntTable2(fn, a, b, fa, fb, error, y, 0, outFn);
        outFn(b, y + r);
    }
}