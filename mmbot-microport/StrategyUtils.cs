internal static class StrategyUtils
{
    public static double CalcOrderSize(double stA, double actualAmount, double newAmount)
    {
        return newAmount - actualAmount;
    }
}