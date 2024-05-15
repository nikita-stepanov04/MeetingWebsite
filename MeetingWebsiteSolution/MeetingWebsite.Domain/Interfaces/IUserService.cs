using MeetingWebsite.Domain.Models;

namespace MeetingWebsite.Domain.Interfaces
{
    public interface IUserService
    {
        ValueTask<User?> FindByIdAsync(long id);

        Task<User> CreateAsync(User entity);

        ValueTask<User> UpdateAsync(User entity);

        ValueTask<User> DeleteAsync(User entity);

        IEnumerable<User> GetEnumerable();

        Task<int> SaveChangesAsync();
    }
}
