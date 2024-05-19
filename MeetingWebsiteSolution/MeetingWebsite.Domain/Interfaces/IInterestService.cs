using MeetingWebsite.Domain.Models;

namespace MeetingWebsite.Domain.Interfaces
{
    public interface IInterestService
    {
        ValueTask<Interest?> FindByIdAsync(long id);

        Task<Interest> CreateAsync(Interest interest);

        Task<List<Interest>> FindByIdsAsync(IEnumerable<long> ids);

        Task<List<Interest>> GetAllAsync();

        Task<int> SaveChangesAsync();
    }
}
