using GamesFinder.Domain.Interfaces.Repositories;
using GamesFinder.Orchestrator.Domain.Classes.Entities;
using GamesFinder.Orchestrator.Domain.Interfaces.Services;
using GamesFinder.Orchestrator.Publisher;

namespace GamesFinder.Orchestrator.Services;

public class SteamService : GamesWithOffersService, ISteamService
{
  private readonly SteamScrapingPublisher _steamScrapingPublisher;
  public SteamService(IGameRepository<Game> gameRepository, IGameOfferRepository<GameOffer> gameOfferRepository, SteamScrapingPublisher steamScrapingPublisher)
    : base(gameRepository, gameOfferRepository)
  {
    _steamScrapingPublisher = steamScrapingPublisher;
  }
  
  public async Task<IEnumerable<int>> GetAllGamesSteamIdsAsync()
  {
    return await _gameRepository.GetAllSteamIdsAsync();
  }

  public async Task<long> ScrapIdsAsync(IEnumerable<int> gamesIds, bool updateExisting = false)
  {
    var ids = updateExisting
      ? gamesIds
      : gamesIds.Except(await _gameRepository.GetAllSteamIdsAsync());

    await _steamScrapingPublisher.PublishSteamScrapeTaskAsync(ids.ToList(), updateExisting);
    return ids.Count();
  }

  public async Task<long> UpdateSteamOffersAsync(IEnumerable<int> gamesIds)
  {
    throw new NotImplementedException();
  }
}
