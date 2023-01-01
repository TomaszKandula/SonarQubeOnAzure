using SonarQubeProxy.Services.HttpClientService.Models;

namespace SonarQubeProxy.Services.HttpClientService.Abstractions;

public interface IHttpClientService
{
    Task<ExecutionResult> Execute(Models.Configuration configuration, CancellationToken cancellationToken = default);

    Task<T> Execute<T>(Models.Configuration configuration, CancellationToken cancellationToken = default);
}