using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace MMBotGA.data.provider
{
    internal class JsonConfiguredDataProvider : FixedDataProvider
    {
        protected override DataProviderSettings Settings =>
            JsonConvert.DeserializeObject<DataProviderSettings>(File.ReadAllText("allocations.json"));
            
    }
}