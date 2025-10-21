﻿using System.ComponentModel;
using GamesFinder.Orchestrator.Domain.Interfaces.Infrastructure;
using GamesFinder.Orchestrator.Domain.Interfaces.Services;
using GamesFinder.Orchestrator.Publisher.RabbitMQ;
using GamesFinder.Orchestrator.Publisher.Redis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using GamesFinder.Orchestrator.Domain.Classes.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace GamesFinder.Orchestrator.Consumers;

public class SteamWorkerConsumer : BackgroundService, IBrockerConsumer
{
  private readonly Lazy<Task<IConnection>> _lazyConnection;
  private readonly RabbitMqConfig _config;
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<SteamWorkerConsumer> _logger;
  private readonly RedisCacheDB _redis;

  public SteamWorkerConsumer(RabbitMqConfig config, IServiceProvider serviceProvider, ILogger<SteamWorkerConsumer> logger, RedisCacheDB redis)
  {
    _config = config;
    _serviceProvider = serviceProvider;
    _logger = logger;
    _redis = redis;

    _logger.LogInformation("SteamWorkerConsumer initialized: " + _config.HostName + "," + _config.Port);

    _lazyConnection = new Lazy<Task<IConnection>>(async () =>
      {
        var factory = new ConnectionFactory
        {
          HostName = _config.HostName,
          Port = _config.Port,
          UserName = _config.UserName,
          Password = _config.Password
        };

        return await factory.CreateConnectionAsync();
      });
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    var connection = await _lazyConnection.Value;
    var channel = await connection.CreateChannelAsync();

    var consumer = new AsyncEventingBasicConsumer(channel);
    consumer.ReceivedAsync += async (_, ea) =>
    {
      var scope = _serviceProvider.CreateScope();
      ISteamService steamService = scope.ServiceProvider.GetRequiredService<ISteamService>();


      var message = Encoding.UTF8.GetString(ea.Body.ToArray());
      var result = JsonSerializer.Deserialize<RedisResultNotification>(message);
      if (result == null) return;

      _logger.LogInformation($"Recieved result from steam worker with redis key: {result}");

      var games = await _redis.ListRangeAsync<Game>(result.RedisResultKey);
      if (games is null)
      {
        _logger.LogError("No games were found in result!");
        return;
      }

      await steamService.SaveManyAsync(games);
      await _redis.ClearKey(result.RedisResultKey);

      await channel.BasicAckAsync(ea.DeliveryTag, false);
    };

    await channel.BasicConsumeAsync(_config.SteamResultsQueue, false, consumer);
    await Task.Delay(Timeout.Infinite, stoppingToken);
  }

  public async ValueTask DisposeAsync()
  {
    if (_lazyConnection.IsValueCreated)
    {
      var connection = await _lazyConnection.Value;
      await connection.DisposeAsync();
    }
  }
  
  private class RedisResultNotification
  {
    public string TaskId { get; set; } = string.Empty;
    public string RedisResultKey { get; set; } = string.Empty;
  }
}
