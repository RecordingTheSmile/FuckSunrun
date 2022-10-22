using System;
using Flurl.Http.Configuration;

namespace FuckSunrun.Common.HttpClientFactory
{
    public class MyHttpClientFactory:DefaultHttpClientFactory
    {
        public override HttpClient CreateHttpClient(HttpMessageHandler handler)
        {
            var client = base.CreateHttpClient(handler);

            client.Timeout = TimeSpan.FromMinutes(2);

            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("FuckSunrun/1.0");

            return client;
        }

        public override HttpMessageHandler CreateMessageHandler()
        {
            var handler = base.CreateMessageHandler() as HttpClientHandler;

            if(handler == null)
            {
                handler = new HttpClientHandler();
            }

            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
            return new MyHttpClientRetryPolicy
            {
                InnerHandler=handler
            };
        }
    }
}

