namespace SonarQubeProxy.Configuration;

using System;
using System.Net.Http;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Models;
using Services.LoggerService;
using Services.MetricsService;
using Services.HttpClientService;

[ExcludeFromCodeCoverage]
public static class Dependencies
{
	public static void RegisterDependencies(this IServiceCollection services, IConfiguration configuration, IHostEnvironment? environment = default)
	{
		SetupAppSettings(services, configuration);
		SetupLogger(services);
		SetupServices(services);
		if (environment != null)
			SetupRetryPolicyWithPolly(services, configuration, environment);
	}

	private static void SetupAppSettings(IServiceCollection services, IConfiguration configuration) 
	{
		services.AddSingleton(configuration.GetSection(nameof(SonarQube)).Get<SonarQube>());
		services.AddSingleton(configuration.GetSection(nameof(ApplicationPaths)).Get<ApplicationPaths>());
	}

	private static void SetupLogger(IServiceCollection services) 
		=> services.AddSingleton<ILoggerService, LoggerService>();

	private static void SetupServices(IServiceCollection services) 
	{
		services.AddHttpContextAccessor();
		services.AddScoped<HttpClient>();
		services.AddScoped<IMetricsService, MetricsService>();
		services.AddScoped<IHttpClientService, HttpClientService>();
	}

	private static void SetupRetryPolicyWithPolly(IServiceCollection services, IConfiguration configuration, IHostEnvironment? environment)
	{
		var applicationPaths = configuration.GetSection(nameof(ApplicationPaths)).Get<ApplicationPaths>();
		services.AddHttpClient("RetryHttpClient", options =>
		{
			options.BaseAddress = new Uri(environment.IsDevelopment() 
				? applicationPaths.DevelopmentOrigin 
				: applicationPaths.DeploymentOrigin);
			options.DefaultRequestHeaders.Add("Accept", "application/json");
			options.Timeout = TimeSpan.FromMinutes(5);
			options.DefaultRequestHeaders.ConnectionClose = true;
		}).AddPolicyHandler(HttpPolicyHandlers.SetupRetry());
	}
}