namespace GamesFinder.Orchestrator.Domain.Classes;

public class SteamOptions
{
  public string DomainName { get; }
  public string ApiKey { get; }

  public SteamOptions(string domainName, string apiKey)
  {
    DomainName = domainName;
    ApiKey = apiKey;
  }
}