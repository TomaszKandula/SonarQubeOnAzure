using System.Diagnostics.CodeAnalysis;
using SonarQubeProxy.WebApi.Services.HttpClientService.Abstractions;

namespace SonarQubeProxy.WebApi.Services.HttpClientService.Models;

[ExcludeFromCodeCoverage]
public class BearerAuthentication : IAuthentication
{
    public string Token { get; set; } = "";
}