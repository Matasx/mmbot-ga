/// <summary>
/// Current asset value
/// </summary>
/// <param name="Bid">The first bid</param>
/// <param name="Ask">The first ask</param>
/// <param name="Last">Last price</param>
/// <param name="Time">Time when read</param>
internal record Ticker(
    double Bid,
    double Ask,
    double Last,
    long Time
);