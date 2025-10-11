using GamesFinder.Orchestrator.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GamesFinder.Orchestrator.Domain.Classes.Entities;

public class Game(
  string name,
  int steamID,
  List<GameOffer>? initialOffers = null,
  string? description = null,
  string? steamUrl = null,
  string? headerImage = null
    ) : Entity
{
  [BsonElement("name")]
  public string Name { get; set; } = name;

  [BsonElement("steam_url")]
  public string? SteamURL { get; set; } = steamUrl;
  [BsonElement("steam_id")]
  public int SteamID { get; set; } = steamID;
  [BsonElement("in_packages")]
	public List<int> InPackages { get; set; } = new();
	[BsonElement("isDLC")]
	public bool IsDLC { get; set; }
  [BsonElement("description")]
  public string? Description { get; set; } = description;
  [BsonElement("header_image")]
  public string? HeaderImage { get; set; } = headerImage;
  [BsonIgnore]
  public List<GameOffer> Offers = initialOffers ?? new List<GameOffer>();
  [BsonElement("is_released")]
  public bool IsReleased { get; set; }
  [BsonElement("initial_prices")]
  public Dictionary<ECurrency, decimal> InitialPrices = new();

}