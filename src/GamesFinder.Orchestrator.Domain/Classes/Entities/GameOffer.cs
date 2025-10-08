using GamesFinder.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using static GamesFinder.Orchestrator.Domain.Classes.Entities.Game;
namespace GamesFinder.Orchestrator.Domain.Classes.Entities;

public class GameOffer : Entity
{
    [BsonElement("game_id")]
    [BsonRepresentation(BsonType.String)]
    public Guid GameId { get; set; }
    [BsonElement("vendors_game_id")]
    public string VendorsGameId { get; set; }
    [BsonRepresentation(BsonType.String)]
    [BsonElement("vendor")]
    public EVendor Vendor { get; set; }
    [BsonElement("vendors_url")]
    public string VendorsUrl { get; set; }
    [BsonElement("available")]
    public bool Available { get; set; }
    [BsonElement("price")]
    [BsonDictionaryOptions(DictionaryRepresentation.Document)]
    public List<PriceRange> Prices { get; set; }

    public GameOffer(Guid gameId, EVendor vendor, string vendorsGameId, string vendorsUrl, List<PriceRange> prices, bool available = false)
    {
        GameId = gameId;
        Vendor = vendor;
        VendorsGameId = vendorsGameId;
        VendorsUrl = vendorsUrl;
        Available = available;
        Prices = prices;
    }
    
    //TODO: Configure Equals for object comparing

}