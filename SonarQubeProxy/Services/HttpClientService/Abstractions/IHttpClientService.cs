namespace SonarQubeProxy.Services.HttpClientService.Abstractions;

using System.Threading;
using System.Threading.Tasks;
using Models;

public interface IHttpClientService
{
    Task<ExecutionResult> Execute(Configuration configuration, CancellationToken cancellationToken = default);
}