namespace GamesFinder.Orchestrator.Repositories;

public class MongoDBSettings
{
  public string ConnectionString { get; set; } = default!;
  public string Database { get; set; } = default!;
}
