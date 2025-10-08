using GamesFinder.Orchestrator.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GamesFinder.Orchestrator.Domain.Classes.Entities;

public class Game : Entity
{
	[BsonElement("name")]
	public string Name { get; set; }

	[BsonElement("steam_url")]
	public string? SteamURL { get; set; }
	[BsonElement("game_ids")]
	public int SteamID { get; set; }
	[BsonElement("in_packages")]
	public List<int> InPackages { get; set; } = new();
	[BsonElement("isDLC")]
	public bool IsDLC { get; set; }
	[BsonElement("description")]
	public string? Description { get; set; }
	[BsonElement("header_image")]
	public string? HeaderImage { get; set; }
	[BsonIgnore]
	public List<GameOffer> Offers = new();
	[BsonElement("price_range")]
	public PriceRange price {get; set;}

	public Game(
		string name,
		int steamID,
		List<GameOffer>? initialOffers = null,
		string? description = null,
		string? steamUrl = null,
		string? headerImage = null
		)
	{
		Name = name;
		Description = description;
		Offers = initialOffers ?? new List<GameOffer>();
		SteamID = steamID;
		SteamURL = steamUrl;
		HeaderImage = headerImage;
	}

	public class PriceRange
	{
		[BsonElement("initial")]
		public decimal? Initial { get; set; }
		[BsonElement("current")]
		public decimal? Current { get; set; }

		public ECurrency? Currency { get; set; }

		public PriceRange(decimal? initial, decimal? current, ECurrency? currency)
		{
			Initial = initial;
			Current = current;
			Currency = currency;
		}
	}
}