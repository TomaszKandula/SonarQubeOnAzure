namespace SonarQubeProxy.WebApi.Services.LoggerService;

public interface ILoggerService
{
    void LogDebug(string message);

    void LogError(string message);

    void LogInformation(string message);

    void LogWarning(string message);

    void LogCriticalError(string message);
}