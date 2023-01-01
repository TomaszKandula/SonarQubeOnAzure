using SonarQubeProxy.Services.HttpClientService.Abstractions;
using SonarQubeProxy.Services.LoggerService;

namespace SonarQubeProxy.Services.HttpClientService;

public class HttpClientServiceFactory : IHttpClientServiceFactory
{
    public IHttpClientService Create(bool allowAutoRedirect, ILoggerService loggerService)
    {
        var handler = new HttpClientHandler { AllowAutoRedirect = allowAutoRedirect };
        var client = new HttpClient(handler);
        return new HttpClientService(client, loggerService);
    }
}