using System.Net.Http.Headers;

namespace SonarQubeProxy.WebApi.Services.HttpClientService.Models;

public class HttpContentResult
{
    public MediaTypeHeaderValue? ContentType { get; set; }

    public byte[]? Content { get; set; }
}