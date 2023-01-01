using System.Diagnostics.CodeAnalysis;
using SonarQubeProxy.WebApi.Services.HttpClientService.Abstractions;

namespace SonarQubeProxy.WebApi.Services.HttpClientService.Models;

[ExcludeFromCodeCoverage]
public class Configuration
{
    public string Url { get; set; } = "";

    public string Method { get; set; } = "";

    public IDictionary<string, string>? Headers { get; set; }

    public IDictionary<string, string>? QueryParameters { get; set; }

    public IAuthentication? Authentication { get; set; }

    public IPayloadContent? PayloadContent { get; set; }
}