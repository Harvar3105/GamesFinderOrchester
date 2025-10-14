using System;

namespace GamesFinder.Orchestrator.Domain.Interfaces.Infrastructure;

public interface IBrockerConsumer
{
  // protected Task ExecuteAsync(CancellationToken cancellationToken);
  ValueTask DisposeAsync();
}
