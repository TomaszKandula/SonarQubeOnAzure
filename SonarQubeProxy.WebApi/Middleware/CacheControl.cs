using System.Diagnostics.CodeAnalysis;
using Microsoft.Net.Http.Headers;

namespace SonarQubeProxy.Middleware;

[ExcludeFromCodeCoverage]
public class CacheControl
{
    private readonly RequestDelegate _requestDelegate;

    public CacheControl(RequestDelegate requestDelegate) => _requestDelegate = requestDelegate;

    public async Task InvokeAsync(HttpContext httpContext)
    {
        httpContext.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
        {
            NoCache = true
        };

        await _requestDelegate(httpContext);
    }
}