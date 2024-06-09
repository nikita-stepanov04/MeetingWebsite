using Microsoft.Extensions.Caching.Distributed;

namespace MeetingWebsite.Application.Interfaces
{
    public interface IDistributedMeetingCache : IDistributedCache
    {
        Task SetRecordAsync<T>(string key, T value,
            TimeSpan? absoluteExpireTime = null,
            TimeSpan? unusedExpireTime = null) where T : class;

        Task<T?> GetRecordAsync<T>(string key) where T : class;
    }
}
