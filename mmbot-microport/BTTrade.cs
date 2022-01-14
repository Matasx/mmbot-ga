internal record BTTrade(
    long Time,
    double Price,
    double Size = 0,
    double NormProfit = 0,
    double NormAccum = 0,
    double NeutralPrice = 0,
    double OpenPrice = 0,
    double Pl = 0,
    double Pos = 0,
    double Bal = 0,
    double NormProfitTotal = 0,
    double UnspendBalance = 0,
    BTEvent BtEvent = BTEvent.NoEvent,

    string Info = null
);