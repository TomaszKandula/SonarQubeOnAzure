using System.Diagnostics.CodeAnalysis;
using SonarQubeProxy.Services.HttpClientService.Abstractions;

namespace SonarQubeProxy.Services.HttpClientService.Models;

[ExcludeFromCodeCoverage]
public class ContentString : IPayloadContent
{
    public object? Payload { get; set; }
}