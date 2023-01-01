using SonarQubeProxy.WebApi.Services.LoggerService;

namespace SonarQubeProxy.WebApi.Services.HttpClientService.Abstractions;

public interface IHttpClientServiceFactory
{
    IHttpClientService Create(bool allowAutoRedirect, ILoggerService loggerService);
}