using Downloader.Core.Core;

namespace MMBotGA;

internal static class DefaultDownloaderExtensions
{
    public static string GetFile(this DefaultDownloader downloader, DownloadTask downloadTask)
    {
        downloader.Download(downloadTask);
        return downloadTask.ToFileName();
    }
}