using System;

namespace GamesFinder.Orchestrator.Domain.Interfaces.Infrastructure;

public interface IBrockerPublisher
{
  Task PublishAsync<T>(T message, string? queueName = null);
}
