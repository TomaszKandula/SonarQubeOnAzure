using SonarQubeProxy.WebApi.Services.HttpClientService.Models;

namespace SonarQubeProxy.WebApi.Services.HttpClientService.Abstractions;

public interface IHttpClientService
{
    Task<ExecutionResult> Execute(Models.Configuration configuration, CancellationToken cancellationToken = default);

    Task<T> Execute<T>(Models.Configuration configuration, CancellationToken cancellationToken = default);
}