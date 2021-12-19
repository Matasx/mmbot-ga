using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MMBotGA.api
{
    public class LoggingHandler : DelegatingHandler
    {
        public LoggingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Request: {request}");
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            Console.WriteLine($"Response: {response.StatusCode}");
            Console.WriteLine();

            return response;
        }
    }
}
