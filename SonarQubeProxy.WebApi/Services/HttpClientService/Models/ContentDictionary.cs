using System.Diagnostics.CodeAnalysis;
using SonarQubeProxy.WebApi.Services.HttpClientService.Abstractions;

namespace SonarQubeProxy.WebApi.Services.HttpClientService.Models;

[ExcludeFromCodeCoverage]
public class ContentDictionary : IPayloadContent
{
    public IDictionary<string, string> Payload { get; set; } = new Dictionary<string, string>();
}