using MeetingWebsite.Domain.Models;

namespace MeetingWebsite.Domain.Interfaces
{
    public interface IInterestService
    {
        Task<Interest?> FindByIdAsync(long id);

        Task<Interest> CreateAsync(Interest interest);

        Task<IEnumerable<Interest>> FindByIdsAsync(IEnumerable<long> ids);

        Task<List<Interest>> GetAllAsync();

        Task<int> SaveChangesAsync();
    }
}
