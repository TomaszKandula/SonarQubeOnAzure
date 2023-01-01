using System.Diagnostics.CodeAnalysis;
using SonarQubeProxy.WebApi.Services.HttpClientService.Abstractions;

namespace SonarQubeProxy.WebApi.Services.HttpClientService.Models;

[ExcludeFromCodeCoverage]
public class BasicAuthentication : IAuthentication
{
    public string Login { get; set; } = "";

    public string Password { get; set; } = "";
}