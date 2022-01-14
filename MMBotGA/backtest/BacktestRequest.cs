using MMBot.Api.dto;

namespace MMBotGA.backtest
{
    internal class BacktestRequest
    {
        public GenTradesRequest GenTradesRequest { get; set; }
        public RunRequest RunRequest { get; set; }
    }
}
