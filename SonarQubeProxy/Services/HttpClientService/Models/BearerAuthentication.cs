namespace SonarQubeProxy.Services.HttpClientService.Models;

using System.Diagnostics.CodeAnalysis;
using Abstractions;

[ExcludeFromCodeCoverage]
public class BearerAuthentication : IAuthentication
{
    public string Token { get; set; } = "";
}