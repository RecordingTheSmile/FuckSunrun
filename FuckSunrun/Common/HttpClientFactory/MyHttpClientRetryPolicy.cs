using System;
using Polly;
using Polly.Timeout;

namespace FuckSunrun.Common.HttpClientFactory
{
    internal class MyHttpClientRetryPolicy : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var retryPolicy = Polly.Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<HttpRequestException>()
                .Or<TimeoutRejectedException>()
                .WaitAndRetryAsync(new[]{
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(20),
                    TimeSpan.FromSeconds(30)
                });

            var timeoutPolicy = Polly.Policy.TimeoutAsync(15, TimeoutStrategy.Optimistic);


            return retryPolicy.WrapAsync(timeoutPolicy).ExecuteAsync(ct => base.SendAsync(request, ct), cancellationToken);
        }
    }
}

