using System.Diagnostics.CodeAnalysis;
using SonarQubeProxy.Services.HttpClientService.Abstractions;

namespace SonarQubeProxy.Services.HttpClientService.Models;

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