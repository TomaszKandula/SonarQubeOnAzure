using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SonarQubeProxy.WebApi.Errors;
using SonarQubeProxy.WebApi.Services.MetricsService;

namespace SonarQubeProxy.WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[AllowAnonymous]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ApplicationError), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApplicationError), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApplicationError), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApplicationError), StatusCodes.Status422UnprocessableEntity)]
[ProducesResponseType(typeof(ApplicationError), StatusCodes.Status500InternalServerError)]
public class Metrics : ControllerBase
{
    private readonly IMetricsService _metricsService;

    public Metrics(IMetricsService metricsService) => _metricsService = metricsService;

    /// <summary>
    /// Returns badge from SonarQube server for given project name and metric type.
    /// All badges have the same style.
    /// </summary>
    /// <param name="project">SonarQube analysis project name</param>
    /// <param name="metric">SonarQube metric type</param>
    /// <returns>SonarQube badge</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetrics([FromQuery] string project, string metric)
        => await _metricsService.GetMetrics(project, metric);

    /// <summary>
    /// Returns large quality gate badge from SonarQube server for given project name.
    /// </summary>
    /// <param name="project">SonarQube analysis project name</param>
    /// <returns>SonarQube badge</returns>
    [HttpGet("Quality")]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQualityGate([FromQuery] string project)
        => await _metricsService.GetQualityGate(project);
}