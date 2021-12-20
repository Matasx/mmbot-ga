using System.Collections.Generic;

namespace MMBotGA.api.endpoints
{
    internal interface IApiEndpointProvider
    {
        IEnumerable<ApiEndpoint> GetApiEndpoints();
    }
}