using System.Diagnostics.CodeAnalysis;
using Microsoft.OpenApi.Models;

namespace SonarQubeProxy.WebApi.Configuration;

[ExcludeFromCodeCoverage]
public static class SwaggerSupport
{
    private const string DocVersion = "v1";

    private const string ApiName = "SonarQube Proxy API";

    public static void SetupSwaggerOptions(this IServiceCollection services, IHostEnvironment? environment)
    {
        if (environment.IsProduction())
            return;

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(DocVersion, new OpenApiInfo
            {
                Title = ApiName, 
                Version = DocVersion
            });
        });
    }

    public static void SetupSwaggerUi(this IApplicationBuilder builder, IConfiguration configuration, IHostEnvironment? environment)
    {
        if (environment.IsProduction())
            return;

        const string url = $"/swagger/{DocVersion}/swagger.json";
        builder.UseSwagger();
        builder.UseSwaggerUI(options => options.SwaggerEndpoint(url, ApiName));
    }
}