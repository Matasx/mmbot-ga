using Downloader.Core.Core;

namespace MMBotGA.data
{
    internal class DataProviderDateSettings
    {
        public bool Automatic { get; set; }
        public DateTimeRange Backtest { get; set; }
        public DateTimeRange Control { get; set; }
    }
}