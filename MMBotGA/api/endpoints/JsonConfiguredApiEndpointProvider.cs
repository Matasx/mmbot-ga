using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace MMBotGA.api.endpoints
{
    internal class JsonConfiguredApiEndpointProvider : IApiEndpointProvider
    {
        public IEnumerable<ApiEndpoint> GetApiEndpoints() =>
            JsonConvert.DeserializeObject<ApiEndpoint[]>(File.ReadAllText("endpoints.json"));
    }
}