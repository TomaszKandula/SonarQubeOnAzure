namespace SonarQubeProxy.Services.Caching.Metrics;

using System;
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
public class MetricsCache : IMetricsCache
{
    private readonly IRedisDistributedCache _redisDistributedCache;

    private readonly IHttpClientService _httpClientService;

    private readonly ILoggerService _loggerService;

    private readonly SonarQube _sonarQube;

    public MetricsCache(IHttpClientService httpClientService, IRedisDistributedCache redisDistributedCache, 
        ILoggerService loggerService, SonarQube sonarQube)
    {
        _httpClientService = httpClientService;
        _redisDistributedCache = redisDistributedCache;
        _loggerService = loggerService;
        _sonarQube = sonarQube;
    }

    public async Task<IActionResult> GetMetrics(string project, string metric, bool noCache = false)
    {
        ValidateArguments(new Dictionary<string, string>
        {
            [nameof(project)] = project, 
            [nameof(metric)] = metric
        });

        var key = $"{project}/{metric}";
        var requestUrl = $"{_sonarQube.Server}/api/project_badges/measure?project={project}&metric={metric}";
        if (noCache)
            return await ExecuteRequest(requestUrl);

        var value = await _redisDistributedCache.GetObjectAsync<FileContentResult>(key);
        if (value is not null) return value;

        value = await ExecuteRequest(requestUrl);
        await _redisDistributedCache.SetObjectAsync(key, value);

        return value;
    }

    public async Task<IActionResult> GetQualityGate(string project, bool noCache = false)
    {
        ValidateArguments(new Dictionary<string, string>
        {
            [nameof(project)] = project
        });
            
        var requestUrl = $"{_sonarQube.Server}/api/project_badges/quality_gate?project={project}";
        if (noCache)
            return await ExecuteRequest(requestUrl);

        var value = await _redisDistributedCache.GetObjectAsync<FileContentResult>(project);
        if (value is not null) return value;

        value = await ExecuteRequest(requestUrl);
        await _redisDistributedCache.SetObjectAsync(project, value);

        return value;
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
        throw new Exception(message);
    }
}