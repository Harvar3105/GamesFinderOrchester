using System;
using System.Text.Json;
using StackExchange.Redis;

namespace GamesFinder.Orchestrator.Publisher.Redis;

public class RedisCacheDB
{
  private readonly IDatabase _db;

  public RedisCacheDB(IConnectionMultiplexer redis)
  {
    _db = redis.GetDatabase();
  }

  public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
  {
    var json = JsonSerializer.Serialize(value);
    await _db.StringSetAsync(key, json, expiry);
  }

  public async Task<T?> GetAsync<T>(string key)
  {
    var json = await _db.StringGetAsync(key);
    return json.HasValue
    ? JsonSerializer.Deserialize<T>(json.ToString())
    : default;
  }

  public async Task<IEnumerable<T>?> ListRangeAsync<T>(string key)
  {
    var json = await _db.ListRangeAsync(key);
    return json
    .Select(i => i.HasValue
        ? JsonSerializer.Deserialize<T>(i.ToString())
        : default)
    .Where(x => x is not null)!;
  }
  
  public async Task<bool> ClearKey(string key)
  {
    return await _db.KeyDeleteAsync(key);
  }
}
