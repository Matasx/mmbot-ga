using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using MMBotGA.api.endpoints;

namespace MMBotGA.api
{
    internal static class ApiDefinitions
    {
        private static IEnumerable<ApiEndpoint> Endpoints => new JsonConfiguredApiEndpointProvider().GetApiEndpoints();

        public static ApiLease GetLease()
        {
            return new ApiLease(Endpoints
                .Select(x => CreateBackend(x.LeaseCount, x.Url, x.Username, x.Password))
                .ToArray()
            );
        }

        private static LeasableApi CreateBackend(int leaseCount, string url, string username, string password)
        {
            return new LeasableApi(leaseCount, new Api(url, new HttpClient(new HttpClientHandler()
            {
                Credentials = new NetworkCredential(username, password),
                PreAuthenticate = true,
                MaxConnectionsPerServer = 20
            })
            {
                Timeout = TimeSpan.FromSeconds(10)
            }));
        }
    }
}