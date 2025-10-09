using System;
using GamesFinder.Domain.Enums;
using GamesFinder.Domain.Interfaces.Repositories;
using GamesFinder.Orchestrator.Domain.Classes.Entities;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace GamesFinder.Orchestrator.Repositories.Repositories;

public class GameOfferRepository : Repository<GameOffer>, IGameOfferRepository<GameOffer>
{
  public GameOfferRepository(IMongoDatabase database, ILogger<Repository<GameOffer>> logger) : base(database, "game_offers", logger)
  {
  }

  public async Task<ICollection<GameOffer>?> GetByGameIdAsync(Guid gameId)
  {
    return await _collection
      .Find(offer => offer.GameId == gameId)
      .ToListAsync();
  }

  public async Task<Dictionary<Guid, List<GameOffer>>?> GetByGameIdsAsync(IEnumerable<Guid> gameIds)
  {
    if (gameIds == null || !gameIds.Any())
      return null;

    var idList = gameIds.ToList();

    var filter = Builders<GameOffer>.Filter.In(offer => offer.GameId, idList);

    var offers = await _collection.Find(filter).ToListAsync();

    if (!offers.Any())
      return null;

    var grouped = offers
      .GroupBy(o => o.GameId)
      .ToDictionary(g => g.Key, g => g.ToList());

    return grouped;
  }

  public async Task<GameOffer?> GetByVandorsIdAsync(string vendorsGameId)
  {
    return await _collection
      .Find(g => g.VendorsGameId.Equals(vendorsGameId, StringComparison.OrdinalIgnoreCase))
      .FirstOrDefaultAsync();
  }

  public async Task<ICollection<GameOffer>?> GetByVendorAsync(EVendor vendor)
  {
    return await _collection
      .Find(offer => offer.Vendor == vendor)
      .ToListAsync();
  }
}
