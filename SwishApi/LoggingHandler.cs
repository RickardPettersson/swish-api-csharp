using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SwishApi
{
    public class LoggingHandler : DelegatingHandler
    {
        private bool _enable;

        public LoggingHandler(HttpMessageHandler innerHandler, bool enable)
            : base(innerHandler)
        {
            _enable = enable;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_enable)
            {
                Console.WriteLine("Request:");
                Console.WriteLine(request.ToString());
                if (request.Content != null)
                {
                    Console.WriteLine(await request.Content.ReadAsStringAsync());
                }
                Console.WriteLine();
            }
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            if (_enable)
            {
                Console.WriteLine("Response:");
                Console.WriteLine(response.ToString());
                if (response.Content != null)
                {
                    Console.WriteLine(await response.Content.ReadAsStringAsync());
                }
                Console.WriteLine();
            }

            return response;
        }
    }
}
