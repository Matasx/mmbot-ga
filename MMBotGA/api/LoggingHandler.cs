using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace MMBotGA.api
{
    public class LoggingHandler : DelegatingHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LoggingHandler));

        public LoggingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Log.Debug($"Request: {request}");
            var response = await base.SendAsync(request, cancellationToken);

            Log.Debug($"Response: {response.StatusCode}");

            return response;
        }
    }
}
