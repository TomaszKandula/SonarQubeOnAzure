using System.Diagnostics.CodeAnalysis;
using SonarQubeProxy.Services.HttpClientService.Abstractions;

namespace SonarQubeProxy.Services.HttpClientService.Models;

[ExcludeFromCodeCoverage]
public class ContentDictionary : IPayloadContent
{
    public IDictionary<string, string> Payload { get; set; } = new Dictionary<string, string>();
}