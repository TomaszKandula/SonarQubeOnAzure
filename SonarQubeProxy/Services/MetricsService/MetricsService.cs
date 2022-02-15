namespace SonarQubeProxy.Services.MetricsService;

using System.Net;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using FluentValidation.Results;
using Models;
using Resources;
using Exceptions;
using LoggerService;
using HttpClientService;
using HttpClientService.Authentication;

[ExcludeFromCodeCoverage]
public class MetricsService : IMetricsService
{
    private readonly IHttpClientService _httpClientService;

    private readonly ILoggerService _loggerService;

    private readonly SonarQube _sonarQube;

    public MetricsService(IHttpClientService httpClientService, ILoggerService loggerService, SonarQube sonarQube)
    {
        _httpClientService = httpClientService;
        _loggerService = loggerService;
        _sonarQube = sonarQube;
    }

    public async Task<IActionResult> GetMetrics(string project, string metric)
    {
        ValidateArguments(new Dictionary<string, string>
        {
            [nameof(project)] = project, 
            [nameof(metric)] = metric
        });

        var requestUrl = $"{_sonarQube.Server}/api/project_badges/measure?project={project}&metric={metric}";
        return await ExecuteRequest(requestUrl);
    }

    public async Task<IActionResult> GetQualityGate(string project)
    {
        ValidateArguments(new Dictionary<string, string>
        {
            [nameof(project)] = project
        });
            
        var requestUrl = $"{_sonarQube.Server}/api/project_badges/quality_gate?project={project}";
        return await ExecuteRequest(requestUrl);
    }

    private static void ValidateArguments(IDictionary<string, string> properties)
    {
        var result = new ValidationResult(new List<ValidationFailure>());
        foreach (var (key, value) in properties)
        {
            if (!string.IsNullOrEmpty(value)) 
                continue;

            var failure = new ValidationFailure(key, ErrorCodes.INVALID_ARGUMENT)
            {
                ErrorCode = nameof(ErrorCodes.INVALID_ARGUMENT)
            };

            result.Errors.Add(failure);
        }

        if (result.Errors.Any())
            throw new ValidationException(result, ErrorCodes.INVALID_ARGUMENT);
    }

    private async Task<FileContentResult> ExecuteRequest(string requestUrl)
    {
        var authentication = new BasicAuthentication
        {
            Login = _sonarQube.Token, 
            Password = string.Empty
        };

        var configuration = new Services.HttpClientService.Models.Configuration
        {
            Url = requestUrl, 
            Method = "GET", 
            Authentication = authentication
        };

        var results = await _httpClientService.Execute(configuration);
        if (results.StatusCode == HttpStatusCode.OK)
            return new FileContentResult(results.Content!, results.ContentType?.MediaType!);

        var message = results.Content is null 
            ? ErrorCodes.ERROR_UNEXPECTED 
            : Encoding.Default.GetString(results.Content);

        _loggerService.LogError($"Received null content with status code: {results.StatusCode}");
        throw new BusinessException(message);
    }
}