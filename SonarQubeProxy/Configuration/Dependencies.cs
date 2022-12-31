using SonarQubeProxy.Services.HttpClientService.Abstractions;

namespace SonarQubeProxy.Configuration;

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services.LoggerService;
using Services.MetricsService;
using Services.HttpClientService;

[ExcludeFromCodeCoverage]
public static class Dependencies
{
	public static void RegisterDependencies(this IServiceCollection services, IConfiguration configuration, IHostEnvironment? environment = default)
	{
		SetupLogger(services);
		SetupServices(services);
		if (environment != null)
			SetupRetryPolicyWithPolly(services, configuration, environment);
	}

	private static void SetupLogger(IServiceCollection services) 
		=> services.AddSingleton<ILoggerService, LoggerService>();

	private static void SetupServices(IServiceCollection services) 
	{
		services.AddHttpContextAccessor();
		services.AddSingleton<IHttpClientServiceFactory>(_ => new HttpClientServiceFactory());
		services.AddScoped<IMetricsService, MetricsService>();
	}

	private static void SetupRetryPolicyWithPolly(IServiceCollection services, IConfiguration configuration, IHostEnvironment? environment)
	{
		var developmentOrigin = configuration.GetValue<string>("DevelopmentOrigin");
		var deploymentOrigin = configuration.GetValue<string>("DeploymentOrigin");
		var origin = environment.IsDevelopment() ? developmentOrigin : deploymentOrigin;

		services.AddHttpClient("RetryHttpClient", options =>
		{
			options.BaseAddress = new Uri(origin);
			options.DefaultRequestHeaders.Add("Accept", "application/json");
			options.Timeout = TimeSpan.FromMinutes(5);
			options.DefaultRequestHeaders.ConnectionClose = true;
		}).AddPolicyHandler(HttpPolicyHandlers.SetupRetry());
	}
}