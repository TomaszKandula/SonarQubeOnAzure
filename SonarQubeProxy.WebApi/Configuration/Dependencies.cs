using System.Diagnostics.CodeAnalysis;
using SonarQubeProxy.Services.HttpClientService;
using SonarQubeProxy.Services.HttpClientService.Abstractions;
using SonarQubeProxy.Services.LoggerService;
using SonarQubeProxy.Services.MetricsService;

namespace SonarQubeProxy.Configuration;

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