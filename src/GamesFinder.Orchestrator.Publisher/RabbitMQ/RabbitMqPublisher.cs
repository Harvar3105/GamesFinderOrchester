using GamesFinder.Orchestrator.Domain.Interfaces.Infrastructure;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace GamesFinder.Orchestrator.Publisher.RabbitMQ;

public class RabbitMqPublisher : IBrockerPublisher
{
  private readonly RabbitMqConfig _config;

  public RabbitMqPublisher(RabbitMqConfig config)
  {
    _config = config;
  }

  public async Task PublishAsync<T>(T message, string? queueName = null)
  {
    var factory = new ConnectionFactory
    {
      HostName = _config.HostName,
      Port = _config.Port,
      UserName = _config.UserName,
      Password = _config.Password
    };

    using var connection = await factory.CreateConnectionAsync();
    using var channel = await connection.CreateChannelAsync();

    var targetQueue = queueName ?? _config.DefaultQueue;

    await channel.QueueDeclareAsync(
      queue: targetQueue,
      durable: true,
      exclusive: false,
      autoDelete: false,
      arguments: null
    );

    var json = JsonSerializer.Serialize(message);
    var body = Encoding.UTF8.GetBytes(json);

    var props = new BasicProperties
      {
        DeliveryMode = DeliveryModes.Persistent
      };

    await channel.BasicPublishAsync(
      exchange: "",
      routingKey: targetQueue,
      mandatory: false,
      basicProperties: props,
      body: body
    );

    Console.WriteLine($"[RabbitMQ] Published to '{targetQueue}': {json}");
  }
}
