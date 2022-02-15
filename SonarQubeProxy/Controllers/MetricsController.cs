namespace SonarQubeProxy.Controllers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Services.MetricsService;
using Models;

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
    /// <param name="noCache">Allows to disable response cache.</param>
    /// <returns>SonarQube badge</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetrics([FromQuery] string project, string metric)
        => await _metricsService.GetMetrics(project, metric);

    /// <summary>
    /// Returns large quality gate badge from SonarQube server for given project name.
    /// </summary>
    /// <param name="project">SonarQube analysis project name</param>
    /// <param name="noCache">Allows to disable response cache.</param>
    /// <returns>SonarQube badge</returns>
    [HttpGet("Quality")]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQualityGate([FromQuery] string project)
        => await _metricsService.GetQualityGate(project);
}