using Microsoft.AspNetCore.Mvc;

namespace SonarQubeProxy.Services.MetricsService;

public interface IMetricsService
{
    Task<IActionResult> GetMetrics(string project, string metric);

    Task<IActionResult> GetQualityGate(string project);
}