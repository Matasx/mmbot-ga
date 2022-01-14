internal enum FeeScheme
{
    /// Fees are subtracted from the currency
    Currency,
    /// Fees are subtracted from the asset
    Assets,
    /// Fees are subtracted from income (buy - assets, sell - currency)
    Income,
    /// Fees are subtracted from outcome (buy - currency, sell - assets)
    Outcome
};