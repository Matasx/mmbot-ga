using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace MMBotGA.data.provider
{
    internal class JsonConfiguredDataProvider : FixedDataProvider
    {
        protected override IEnumerable<Allocation> Allocations =>
            JsonConvert.DeserializeObject<Allocation[]>(File.ReadAllText("allocations.json"));
    }
}