using MeetingWebsite.Domain.Models;

namespace MeetingWebsite.Domain.Interfaces
{
    public interface IUserService
    {
        Task<User?> FindByIdAsync(long id);

        Task<User> CreateAsync(User entity);

        ValueTask<User> UpdateAsync(User entity);

        ValueTask<User> DeleteAsync(User entity);

        Task<int> SaveChangesAsync();
    }
}
