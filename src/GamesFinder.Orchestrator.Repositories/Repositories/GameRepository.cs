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
  public GameRepository(IMongoDatabase database, ILogger<GameRepository> logger) : base(database, "game", logger)
  {
    
  }

  public async Task<Game?> GetBySteamId(int appId)
  {
    try
    {
      return await Collection
        .Find(g => g.SteamID == appId)
        .FirstOrDefaultAsync();
    }
    catch (Exception ex)
    {
      Logger.LogError(ex.Message);
      return null;
    }
  }

  public async Task<List<Game>?> GetByAppIds(IEnumerable<int> appIds)
  {
      try
      {
          var stringedIds = appIds.Select(id => id.ToString());
          var result = await Collection
              .Find(g => g.GameIds.Any(v => stringedIds.Contains(v.RealId))).ToListAsync();

          foreach (var game in result)
          {
              game.Offers = (await _gameOfferRepository.GetByGameIdAsync(game.Id))?.ToList() ?? new List<GameOffer>();
          }
          
          return result;
      }
      catch (Exception e)
      {
          Logger.LogError(e.Message);
          return null;
      }
  }

  public async Task<List<string>> CheckExistManyBySteamIds(List<string> appIds)
  {
      var vendor = EVendor.Steam;
      var existingOnes = await Collection
          .Find(g => appIds.Contains(g.GameIds.First(i => i.Vendor == vendor).RealId))
          .Project(g => g.GameIds.First(i => i.Vendor == vendor).RealId)
          .ToListAsync();
      
      return appIds.Where(id => !existingOnes.Contains(id)).ToList();
  }

  public async Task<bool> ExistsByAppIdAsync(int appId)
  {
      try
      {
          return await Collection
              .Find(g => g.GameIds.Any(v => v.RealId.Equals(appId.ToString()))).AnyAsync();
      }
      catch (Exception ex)
      {
          Logger.LogError(ex.Message);
          return false;
      }
      
  }

  public async Task<bool> ExistsByAppNameAsync(string appName)
  {
      try
      {
          var allProducts = (await Collection
              .Find(_ => true)
              .Project(p => p.Name)
              .ToListAsync()).OrderBy(g => g.Length);
                  
          
          return allProducts.Any(name =>
              appName.Contains(name, StringComparison.OrdinalIgnoreCase));
      }
      catch (Exception e)
      {
          Logger.LogError(e.Message);
          return false;
      }
      
  }

  public async Task<ICollection<Game>?> GetPagedWithFiltersAsync(int page, int pageSize, GamesFilters query)
  {
      try
      {
          var pipeline = new List<BsonDocument>();

          // 1. lookup offers
          pipeline.Add(new BsonDocument("$lookup", new BsonDocument
          {
              { "from", "game_offers" },
              { "localField", "_id" },
              { "foreignField", "game_id" },
              { "as", "offers" }
          }));

          // 2. filter by name
          if (!string.IsNullOrEmpty(query.Search))
          {
              pipeline.Add(new BsonDocument("$match", new BsonDocument(nameof(Game.Name),
                  new BsonDocument("$regex", query.Search).Add("$options", "i"))));
          }

          // 3. switch to offers
          pipeline.Add(new BsonDocument("$unwind", "$offers"));

          // 4. filter by price of any offer
          if (query.PriceCompare is not null)
          {
              var priceExpr = new BsonDocument();
              //TODO: fix not only for EUR
              string pricePath = $"offers.prices.EUR.current";
              switch (query.PriceCompare)
              {
                  case EPriceCompare.Less:
                      priceExpr = new BsonDocument(pricePath, new BsonDocument("$lt", query.PriceValue));
                      break;

                  case EPriceCompare.Greater:
                      priceExpr = new BsonDocument(pricePath, new BsonDocument("$gt", query.PriceValue));
                      break;

                  case EPriceCompare.Equal:
                      priceExpr = new BsonDocument(pricePath, new BsonDocument("$eq", query.PriceValue));
                      break;

                  case EPriceCompare.InRange:
                      priceExpr = new BsonDocument(pricePath, new BsonDocument
                      {
                          { "$gte", query.PriceRangeMin },
                          { "$lte", query.PriceRangeMax }
                      });
                      break;

                  case EPriceCompare.Null:
                      priceExpr = new BsonDocument(pricePath, BsonNull.Value);
                      break;

                  case EPriceCompare.NotNull:
                      priceExpr = new BsonDocument(pricePath, new BsonDocument("$ne", BsonNull.Value));
                      break;

                  default:
                      throw new NotSupportedException($"Unsupported price filter: {query.PriceCompare}");
              }
              
              //TODO: fix not only for EUR
              pipeline.Add(new BsonDocument("$match", new BsonDocument("offers.prices.EUR.current", priceExpr)));
          }

          // 5. connect offers
          pipeline.Add(new BsonDocument("$group", new BsonDocument
          {
              { "_id", "$_id" },
              { "game", new BsonDocument("$first", "$$ROOT") }
          }));

          // 6. sort
          var sortOrder = query.SortOrder == ESort.Descending ? -1 : 1;
          pipeline.Add(new BsonDocument("$sort", new BsonDocument($"game.{query.SortField}", sortOrder)));

          // 7. page
          pipeline.Add(new BsonDocument("$skip", (page - 1) * pageSize));
          pipeline.Add(new BsonDocument("$limit", pageSize));

          var result = await Collection.AggregateAsync<BsonDocument>(pipeline);
          var docs = await result.ToListAsync();
          var games = docs.Select(d => BsonSerializer.Deserialize<Game>(d["game"].AsBsonDocument)).ToList();

          return games;
      }
      catch (Exception ex)
      {
          Logger.LogError(ex.Message);
          return null;
      }
  }

  public async Task<Game?> GetByAppNameAsync(string appName)
  {
      try
      {
          var allProducts = (await Collection
              .Find(_ => true)
              .ToListAsync())
              .OrderBy(g => g.Name.Length);

          var result = allProducts.First(g => appName.Contains(g.Name, StringComparison.OrdinalIgnoreCase));

          if (result != null)
          {
              result.Offers = (await _gameOfferRepository.GetByGameIdAsync(result.Id))?.ToList() ?? new List<GameOffer>();
          }
          
          return result;
      }
      catch (Exception e)
      {
          Logger.LogError(e.Message);
          return null;
      }
      
  }
}