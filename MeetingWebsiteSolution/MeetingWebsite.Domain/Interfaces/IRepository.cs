namespace MeetingWebsite.Domain.Interfaces
{
    public interface IRepository<TEntity, TId>
        where TEntity : class
    {
        ValueTask<TEntity?> FindByIdAsync(TId id);

        Task<TEntity> CreateAsync(TEntity entity);

        ValueTask<TEntity> UpdateAsync(TEntity entity);

        ValueTask<TEntity> DeleteAsync(TEntity entity);

        IEnumerable<TEntity> GetEnumerable();

        Task<int> SaveChangesAsync();
    }
}
