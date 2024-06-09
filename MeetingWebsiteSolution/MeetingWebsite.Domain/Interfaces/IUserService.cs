using MeetingWebsite.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace MeetingWebsite.Domain.Interfaces
{
    public interface IUserService
    {
        Task<User?> FindByIdAsync(long id);

        Task<User> CreateAsync(User entity);

        ValueTask<User> UpdateAsync(User entity);

        ValueTask<User> DeleteAsync(User entity);

        Task<IEnumerable<User>> FindUsersByFiltersAndPagingInfo(
            FilterInfo? filters, PagingInfo pagingInfo, IEnumerable<User> except);

        Task<int> Count();

        Task<int> SaveChangesAsync();
    }
}
