using System.Diagnostics.CodeAnalysis;

namespace SonarQubeProxy.WebApi.Services.MetricsService.Models;

[ExcludeFromCodeCoverage]
public class TokenResponse
{
    public string Token { get; set; } = "";
}
