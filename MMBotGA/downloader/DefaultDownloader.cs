using System.Net.Http;
using Downloader.Core.Core;
using Downloader.Core.Exchange.Binance;
using Downloader.Core.Exchange.Bitfinex;
using Downloader.Core.Exchange.FTX;
using Downloader.Core.Exchange.Kucoin;
using Downloader.Core.Utils;

namespace MMBotGA.downloader
{
    internal class DefaultDownloader
    {
        private readonly DownloadOrchestrator _downloadOrchestrator;

        public DefaultDownloader(IProgress progress)
        {
            var ui = new UserInterface();
            var client = new HttpClient(new TransientErrorRetryHttpClientHandler());
            _downloadOrchestrator = new DownloadOrchestrator(ui, progress, new IGenericDownloader[] {
                new BinanceDownloader(client),
                new BitfinexDownloader(client),
                new FTXDownloader(client),
                new KucoinDownloader(client)
            });
        }

        public string Download(DownloadTask downloadTask)
        {
            return _downloadOrchestrator.Download(downloadTask);
        }
    }
}