using System;

namespace GamesFinder.Orchestrator.Publisher.Redis;

public class RedisConfig
{
  public string Host { get; }
  public int Port { get; }
  public string? Password { get; }
  public int Database { get; }

  public RedisConfig(string host, int port, string? password, int database)
  {
    Host = host;
    Port = port;
    Password = password;
    Database = database;
  }
}
