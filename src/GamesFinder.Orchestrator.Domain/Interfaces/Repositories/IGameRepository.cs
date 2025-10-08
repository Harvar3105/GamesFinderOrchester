
using GamesFinder.Orchestrator.Domain.Classes.Entities;

namespace GamesFinder.Domain.Interfaces.Repositories;

public interface IGameRepository<TEntity> : IRepository<TEntity> where TEntity : Game
{
	Task<Game?> GetBySteamId(int appId);
	Task<List<Game>?> GetByAppIds(IEnumerable<int> appIds);

	Task<List<string>> CheckExistManyBySteamIds(List<string> appIds);
	
	Task<Game?> GetByAppNameAsync(string appName);

	Task<bool> ExistsByAppIdAsync(int appId);
	Task<bool> ExistsByAppNameAsync(string appName);
}