using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Interfaces.Requests;

namespace GamesFinder.Domain.Interfaces.Repositories;

public interface IRepository<TEntity> where TEntity : Entity
{
    Task<bool> SaveAsync(TEntity entity);
    Task<bool> SaveManyAsync(IEnumerable<TEntity> entities);
    public Task<bool> SaveOrUpdateAsync(TEntity entity);
    public Task<bool> SaveOrUpdateManyAsync(IEnumerable<TEntity> entities);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> UpdateAsync(TEntity entity);
    Task<ICollection<TEntity>?> GetAllAsync();
    Task<TEntity?> GetByIdAsync(Guid id);
    public Task<bool> ExistsAsync(Guid id);

    public Task<long> CountAsync();

    public Task<ICollection<TEntity>> GetPagedAsync(int page, int pageSize);
}