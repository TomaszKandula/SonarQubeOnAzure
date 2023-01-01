using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Newtonsoft.Json.Converters;
using Serilog;
using SonarQubeProxy.Configuration;
using SonarQubeProxy.Exceptions;
using SonarQubeProxy.Middleware;

namespace SonarQubeProxy;

[ExcludeFromCodeCoverage]
public class Startup
{
    private readonly IConfiguration _configuration;

    private readonly IHostEnvironment? _environment;

    public Startup(IConfiguration configuration, IHostEnvironment? environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.Converters.Add(new StringEnumConverter()));
        services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ErrorResponses = new ApiVersionException();
        });
        services.AddResponseCompression(options => options.Providers.Add<GzipCompressionProvider>());
        services.RegisterDependencies(_configuration, _environment);
        services.SetupSwaggerOptions(_environment);
        services.SetupDockerInternalNetwork();
    }

    public void Configure(IApplicationBuilder builder)
    {
        builder.UseSerilogRequestLogging();

        builder.UseForwardedHeaders();
        builder.UseHttpsRedirection();

        builder.UseMiddleware<CustomExceptions>();
        builder.UseMiddleware<CacheControl>();

        builder.UseResponseCompression();
        builder.UseRouting();

        builder.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGet("/", context => context.Response.WriteAsync("SonarQube Proxy API"));
        });

        builder.SetupSwaggerUi(_configuration, _environment);
    }
}