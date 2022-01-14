/// <param name="AssetSymbol">Symbol of asset (must match with symbol in balances)</param>
/// <param name="CurrencySymbol">Symbol of currency (must match with symbol in balances)</param>
/// <param name="AssetStep">Smallest change of amount</param>
/// <param name="CurrencyStep">Smallest change of currency</param>
/// <param name="MinSize">Smallest allowed amount</param>
/// <param name="MinVolume">Smallest allowed of volume (price*size)</param>
/// <param name="Fees">Fees of volume 1 (0.12% => 0.0012)</param>
/// <param name="FeeScheme">How fees are handled</param>
/// <param name="Leverage">Leverage if margin trading is involved
///	Default value is 0, which means no margin is available
/// When this field is set, this changes several calculations
/// in report. It doesn't affect trading. By setting leverage also
/// enables short trades. However you still need to set external_assets
/// to specify starting point
/// @note when leverage is set, command 'achieve' expects position,
/// not total assets
/// </param>
/// <param name="InvertPrice">The broker inverts price when currency is quoted instead assets
/// Currently 'deribit' broker inverts price because the position is quoted
/// in currency. This result, that price is returned as 1/x.
///
/// By setting this field to true, all prices will be inverted back
/// when they are put to the report. The broker should also report
/// correct symbol of inverted price
/// </param>
/// <param name="InvertedSymbol">When invert_price is true, the broker should also supply symbol name of inverted price</param>
/// <param name="Simulator">This flag must be true, if the broker is just simulator and doesn't do live trading
/// Simulators are not included into daily performance
/// </param>
/// <param name="PrivateChart">Set this flag to disable of sharing chart data
/// Default settings is not shared, however if storage_broker is used, the chart data can
/// be shared with other users. This flag is copied into trader_state as "private_chat", which can
/// be read by the storage_broker to store chart data which prevents sharing
/// </param>
/// <param name="WalletId">Specifies wallet identifier for this pair
/// This allows to broker to expose how balance is shared between traders.
/// Each pair can use different wallet, so their balances are not shared. If
/// the symbols are from the same wallet, the balance is shared between traders
/// and each trader can allocate part of balance. Default value is "", which is
/// also identified as single wallet
/// </param>
internal record MarketInfo(
    string AssetSymbol,
    string CurrencySymbol,
    double AssetStep,
    double CurrencyStep,
    double MinSize,
    double MinVolume,
    double Fees,
    FeeScheme FeeScheme = FeeScheme.Currency,
    double Leverage = 0,
    bool InvertPrice = false,
    string InvertedSymbol = null,
    bool Simulator = false,
    bool PrivateChart = false,
    string WalletId = null
)
{
    /// <summary>
    /// Adds fees to values
    /// Function updates value to reflect current fee scheme. If the fee scheme
    /// subtracts from the currency, the price is adjusted. If the fee scheme
    /// subtracts from the assets, the size is adjusted
    /// </summary>
    /// <param name="assets">reference to current asset change. Negative value is sell, positive is buy</param>
    /// <param name="price">reference to trade price</param>
    public (double Assets, double Price) AddFees(double assets, double price)
    {
        // always shift price
        price *= 1 - Math.Sign(assets) * Fees;
        switch (FeeScheme)
        {
            case FeeScheme.Assets:
                assets *= 1 + Fees;
                break;

            case FeeScheme.Income:
                if (assets > 0)
                {
                    assets *= 1 + Fees;
                }
                break;

            case FeeScheme.Outcome:
                if (assets < 0)
                {
                    assets *= 1 + Fees;
                }
                break;

            case FeeScheme.Currency:
                break;

            default:
                throw new NotSupportedException($"Fee scheme {FeeScheme} is not supported.");
        }

        if (InvertPrice)
        {
            price = 1 / AdjValue(1 / price, CurrencyStep, Rounded);
        }
        else
        {
            price = AdjValue(price, CurrencyStep, Rounded);
        }
        assets = AdjValue(assets, AssetStep, NearZero);

        return new ValueTuple<double, double>(assets, price);
    }

    public static double Rounded(double v) => Math.Round(v, MidpointRounding.AwayFromZero);
    private static double NearZero(double v) => Math.Sign(v) * Math.Floor(Math.Abs(v));

    public (double Assets, double Price) RemoveFees(double assets, double price)
    {
        price /= 1 - Math.Sign(assets) * Fees;
        switch (FeeScheme)
        {
            case FeeScheme.Assets:
                assets /= 1 + Fees;
                break;

            case FeeScheme.Income:
                if (assets > 0)
                {
                    assets /= 1 + Fees;
                }
                break;

            case FeeScheme.Outcome:
                if (assets < 0)
                {
                    assets /= 1 + Fees;
                }
                break;

            case FeeScheme.Currency:
                break;

            default:
                throw new NotSupportedException($"Fee scheme {FeeScheme} is not supported.");
        }

        return new ValueTuple<double, double>(assets, price);
    }
    public static double AdjValue(double value, double step, Func<double, double> fn)
    {
        if (step == 0) return value;
        return fn(value / step) * step;
    }
}