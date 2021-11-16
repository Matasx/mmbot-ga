using MMBotGA.dto;

namespace MMBotGA
{
    internal class BacktestRequest
    {
        public GenTradesRequest GenTradesRequest { get; set; }
        public RunRequest RunRequest { get; set; }
    }
}
