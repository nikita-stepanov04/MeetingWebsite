using MeetingWebsite.Domain.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace MeetingWebsite.Application.Interfaces
{
    public interface IDistributedMeetingCache : IDistributedCache
    {
        Task SetRecordAsync<T, TId>(string prefix, TId id, T value,
            TimeSpan? absoluteExpireTime = null,
            TimeSpan? unusedExpireTime = null) where T : class;

        Task<T?> GetRecordAsync<T, TId>(string prefix, TId id) where T : class;

        Task<User?> GetUserAsync(string prefix, long id);

        Task SetInterestsAsync(string prefix, IEnumerable<Interest> interests,
            TimeSpan? absoluteExpireTime = null,
            TimeSpan? unusedExpireTime = null);

        Task<List<Interest>?> GetInterestAsync(string prefix);

        Task RemoveRecordAsync<TId>(string prefix, TId id);
    }
}
