namespace GamesFinder.Orchestrator.Publisher.RabbitMQ;

public class RabbitMqConfig
{
  public string HostName { get; }
  public int Port { get; }
  public string DefaultQueue { get; }
  public string UserName { get; }
  public string Password { get; }

  public RabbitMqConfig(string hostName, int port, string defaultQueue, string userName, string password)
  {
    HostName = hostName;
    Port = port;
    DefaultQueue = defaultQueue;
    UserName = userName;
    Password = password;
  }
}