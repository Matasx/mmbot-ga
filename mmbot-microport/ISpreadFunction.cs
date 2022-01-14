internal interface ISpreadFunction
{
    public record Result(
        bool Valid = false,
        double Spread = 0,
        double Center = 0,
        int Trend = 0
    );

    object Start();
    Result Point(object state, double y);
}