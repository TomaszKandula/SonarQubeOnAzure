using System.Diagnostics.CodeAnalysis;
using System.Net;
using Polly;
using Polly.Extensions.Http;

namespace SonarQubeProxy.WebApi.Configuration;

[ExcludeFromCodeCoverage]
public static class HttpPolicyHandlers
{
    public static IAsyncPolicy<HttpResponseMessage> SetupRetry()
    {
        const int retryCount = 3;
        const double durationBetweenRetries = 150;

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TaskCanceledException>()
            .OrResult(response => response.StatusCode 
                is HttpStatusCode.RequestTimeout 
                or HttpStatusCode.BadGateway 
                or HttpStatusCode.GatewayTimeout 
                or HttpStatusCode.ServiceUnavailable
            ).WaitAndRetryAsync(retryCount, count 
                => TimeSpan.FromMilliseconds(durationBetweenRetries * Math.Pow(2, count - 1)));
    }
}