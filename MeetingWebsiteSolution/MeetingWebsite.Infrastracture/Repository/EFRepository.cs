using MeetingWebsite.Domain.Interfaces;
using MeetingWebsite.Infrastracture.Models;

namespace MeetingWebsite.Infrastracture.EFRepository
{
    public class EFRepository<TEntity, TId> : IRepository<TEntity, TId>
        where TEntity : class
    {
        private DataContext _context;
        public EFRepository(DataContext context) =>
            _context = context;

        public ValueTask<TEntity?> FindByIdAsync(TId id)
        {
            return _context.Set<TEntity>().FindAsync(id);
        }

        public async Task<TEntity> CreateAsync(TEntity entity)
        {
            return (await _context.Set<TEntity>().AddAsync(entity)).Entity;
        }

        public ValueTask<TEntity> UpdateAsync(TEntity entity)
        {
            return ValueTask.FromResult(
                _context.Set<TEntity>().Update(entity).Entity);
        }

        public ValueTask<TEntity> DeleteAsync(TEntity entity)
        {
            return ValueTask.FromResult(
                _context.Set<TEntity>().Remove(entity).Entity);
        }

        public IEnumerable<TEntity> GetEnumerable()
        {
            return _context.Set<TEntity>();
        }

        public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
