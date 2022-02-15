namespace SonarQubeProxy.Services.MetricsService;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

public interface IMetricsService
{
    Task<IActionResult> GetMetrics(string project, string metric);

    Task<IActionResult> GetQualityGate(string project);
}