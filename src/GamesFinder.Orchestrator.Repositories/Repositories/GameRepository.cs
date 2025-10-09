using GamesFinder.Domain.Enums;
using GamesFinder.Domain.Interfaces.Repositories;
using GamesFinder.Orchestrator.Domain.Classes.Entities;
using GamesFinder.Orchestrator.Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace GamesFinder.DAL.Repositories;

public class GameRepository : Repository<Game>, IGameRepository<Game>
{
  public GameRepository(IMongoDatabase database, ILogger<GameRepository> logger) : base(database, "games", logger)
  {
    
  }

  public async Task<Game?> GetBySteamId(int steamId)
  {
    try
    {
      return await _collection
        .Find(g => g.SteamID == steamId)
        .FirstOrDefaultAsync();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex.Message);
      return null;
    }
  }


  public async Task<bool> ExistsByAppNameAsync(string appName)
  {
    try
    {
      var allProducts = (await _collection
        .Find(_ => true)
        .Project(p => p.Name)
        .ToListAsync()).OrderBy(g => g.Length);

      return allProducts.Any(name =>
        appName.Contains(name, StringComparison.OrdinalIgnoreCase));
    }
    catch (Exception e)
    {
      _logger.LogError(e.Message);
      return false;
    }
  }

  public async Task<Game?> GetByAppNameAsync(string appName)
  {
    try
    {
      var allProducts = (await _collection
        .Find(_ => true)
        .ToListAsync())
        .OrderBy(g => g.Name.Length);

      return allProducts.FirstOrDefault(g => appName.Contains(g.Name, StringComparison.OrdinalIgnoreCase));
    }
    catch (Exception e)
    {
      _logger.LogError(e.Message);
      return null;
    }
  }

  public async Task<List<(int, bool)>> ExistBySteamIdMany(List<int> steamIds)
  {
    var filter = Builders<Game>.Filter.In(g => g.SteamID, steamIds);
    var existingIds = await _collection
        .Find(filter)
        .Project(g => g.SteamID)
        .ToListAsync();

    return steamIds
        .Select(id => (id, existingIds.Contains(id)))
        .ToList();
  }

  public async Task<bool> ExistsBySteamIdAsync(int steamId)
  {
    var filter = Builders<Game>.Filter.Eq(g => g.SteamID, steamId);
    return await _collection.Find(filter).AnyAsync();
  }
}