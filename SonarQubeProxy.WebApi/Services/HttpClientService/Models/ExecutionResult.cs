using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace SonarQubeProxy.WebApi.Services.HttpClientService.Models;

[ExcludeFromCodeCoverage]
public class ExecutionResult : HttpContentResult
{
    public HttpStatusCode StatusCode { get; set; }
}