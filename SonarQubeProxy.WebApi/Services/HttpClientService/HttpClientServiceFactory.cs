using SonarQubeProxy.WebApi.Services.HttpClientService.Abstractions;
using SonarQubeProxy.WebApi.Services.LoggerService;

namespace SonarQubeProxy.WebApi.Services.HttpClientService;

public class HttpClientServiceFactory : IHttpClientServiceFactory
{
    public IHttpClientService Create(bool allowAutoRedirect, ILoggerService loggerService)
    {
        var handler = new HttpClientHandler { AllowAutoRedirect = allowAutoRedirect };
        var client = new HttpClient(handler);
        return new HttpClientService(client, loggerService);
    }
}