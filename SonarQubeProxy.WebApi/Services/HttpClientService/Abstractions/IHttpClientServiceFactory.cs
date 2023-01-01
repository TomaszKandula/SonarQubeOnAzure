using SonarQubeProxy.Services.LoggerService;

namespace SonarQubeProxy.Services.HttpClientService.Abstractions;

public interface IHttpClientServiceFactory
{
    IHttpClientService Create(bool allowAutoRedirect, ILoggerService loggerService);
}