namespace SonarQubeProxy.Services.HttpClientService.Models;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Abstractions;

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