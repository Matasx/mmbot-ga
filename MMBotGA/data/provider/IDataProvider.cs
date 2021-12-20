using Downloader.Core.Core;
using MMBotGA.ga.abstraction;

namespace MMBotGA.data.provider
{
    internal interface IDataProvider
    {
        Batch[] GetBacktestData(IProgress progressCallback);
        Batch[] GetControlData(IProgress progressCallback);
    }
}