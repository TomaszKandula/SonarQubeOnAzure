namespace SonarQubeProxy.Services.CachingService;

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Distributed;
using Models;
using Exceptions;
using Resources;
using Newtonsoft.Json;

[ExcludeFromCodeCoverage]
public class CachingService : ICachingService
{
    private readonly IDistributedCache _distributedCache;

    private readonly AzureRedis _azureRedis;

    public CachingService(IDistributedCache distributedCache, AzureRedis azureRedis)
    {
        _distributedCache = distributedCache;
        _azureRedis = azureRedis;
    }

    public TEntity? GetObject<TEntity>(string key)
    {
        VerifyArguments(new[] { key });

        var cachedValue = _distributedCache.GetString(key);
        return string.IsNullOrEmpty(cachedValue) 
            ? default 
            : JsonConvert.DeserializeObject<TEntity>(cachedValue);
    }

    public async Task<TEntity?> GetObjectAsync<TEntity>(string key)
    {
        VerifyArguments(new[] { key });

        var cachedValue = await _distributedCache.GetStringAsync(key);
        return string.IsNullOrEmpty(cachedValue) 
            ? default 
            : JsonConvert.DeserializeObject<TEntity>(cachedValue);
    }

    public void SetObject<TEntity>(string key, TEntity value, int absoluteExpirationMinute = 0, int slidingExpirationSecond = 0)
    {
        VerifyArguments(new[] { key });
        var serializedObject = JsonConvert.SerializeObject(value);
        _distributedCache.SetString(key, serializedObject, SetDistributedCacheEntryOptions(absoluteExpirationMinute, slidingExpirationSecond));            
    }

    public async Task SetObjectAsync<TEntity>(string key, TEntity value, int absoluteExpirationMinute = 0, int slidingExpirationSecond = 0)
    {
        VerifyArguments(new[] { key });
        var serializedObject = JsonConvert.SerializeObject(value);
        await _distributedCache.SetStringAsync(key, serializedObject, SetDistributedCacheEntryOptions(absoluteExpirationMinute, slidingExpirationSecond));
    }

    public void Remove(string key)
    {
        VerifyArguments(new[] { key });
        _distributedCache.Remove(key);            
    }

    public async Task RemoveAsync(string key)
    {
        VerifyArguments(new[] { key });
        await _distributedCache.RemoveAsync(key);            
    }

    private static void VerifyArguments(IEnumerable<string> arguments)
    {
        foreach (var argument in arguments)
        {
            if (!string.IsNullOrEmpty(argument)) continue;
            const string message = $"The argument '{nameof(argument)}' cannot be null or empty";
            throw new BusinessException(ErrorCodes.ARGUMENT_EMPTY_OR_NULL, message);
        }
    }

    private DistributedCacheEntryOptions SetDistributedCacheEntryOptions(int absoluteExpirationMinute, int slidingExpirationSecond)
    {
        var expirationMinute = absoluteExpirationMinute == 0
            ? _azureRedis.ExpirationMinute
            : absoluteExpirationMinute;

        var expirationSecond = slidingExpirationSecond == 0
            ? _azureRedis.ExpirationSecond
            : slidingExpirationSecond;

        return new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = DateTime.Now.AddMinutes(expirationMinute),
            SlidingExpiration = TimeSpan.FromSeconds(expirationSecond)
        };
    }
}