using System;
using System.Net;
using System.Net.Http;
using MMBotGA.api;

namespace MMBotGA
{
    internal static class ApiDefinitions
    {
        public static ApiLease GetLease()
        {
            return new ApiLease(
                //RPi
                CreateBackend(6, "http://192.168.1.150:10000/admin/api/", "user", "pass"),
                //mtxs
                CreateBackend(10, "http://192.168.1.170:20000/admin/api/", "user", "pass")
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
                Timeout = TimeSpan.FromMinutes(2)
            }));
        }
    }
}