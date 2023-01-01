using System.Diagnostics.CodeAnalysis;
using SonarQubeProxy.Services.HttpClientService.Abstractions;

namespace SonarQubeProxy.Services.HttpClientService.Models;

[ExcludeFromCodeCoverage]
public class BearerAuthentication : IAuthentication
{
    public string Token { get; set; } = "";
}