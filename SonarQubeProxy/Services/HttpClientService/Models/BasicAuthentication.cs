using System.Diagnostics.CodeAnalysis;
using SonarQubeProxy.Services.HttpClientService.Abstractions;

namespace SonarQubeProxy.Services.HttpClientService.Models;

[ExcludeFromCodeCoverage]
public class BasicAuthentication : IAuthentication
{
    public string Login { get; set; } = "";

    public string Password { get; set; } = "";
}