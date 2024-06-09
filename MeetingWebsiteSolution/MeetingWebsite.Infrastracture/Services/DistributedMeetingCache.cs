using MeetingWebsite.Application.Interfaces;
using MeetingWebsite.Infrastracture.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace MeetingWebsite.Infrastracture.Services
{
    public class DistributedMeetingCache : IDistributedMeetingCache
    {
        private readonly IDistributedCache _cache;
        private readonly DataContext _context;

        public DistributedMeetingCache(IDistributedCache cache, DataContext context)
        {
            _cache = cache;
            _context = context;
        }

        public async Task SetRecordAsync<T>(string key, T value,
            TimeSpan? absoluteExpireTime = null,
            TimeSpan? unusedExpireTime = null) where T : class
        {
            var options = GetDistributedCacheEntryOptions(absoluteExpireTime, unusedExpireTime);
            string json = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, json, options);
        }

        public async Task<T?> GetRecordAsync<T>(string key) where T : class
        {
            string? json = await _cache.GetStringAsync(key);
            if (json != null)
            { 
                T entity = JsonSerializer.Deserialize<T>(json)!;
                _context.Set<T>().Attach(entity);
                return entity;
            }
            return null;
        }

        byte[]? IDistributedCache.Get(string key) =>
            _cache.Get(key);

        Task<byte[]?> IDistributedCache.GetAsync(string key, CancellationToken token) =>
            _cache.GetAsync(key, token);

        void IDistributedCache.Refresh(string key) =>
            _cache.Refresh(key);

        Task IDistributedCache.RefreshAsync(string key, CancellationToken token) =>
            _cache.RefreshAsync(key, token);

        void IDistributedCache.Remove(string key) =>
            _cache.Remove(key);

        Task IDistributedCache.RemoveAsync(string key, CancellationToken token) =>
            _cache.RemoveAsync(key, token);

        void IDistributedCache.Set(string key, byte[] value, DistributedCacheEntryOptions options) =>
            _cache.Set(key, value);

        Task IDistributedCache.SetAsync(string key, byte[] value,
            DistributedCacheEntryOptions options, CancellationToken token) =>
                _cache.SetAsync(key, value, options, token);

        private DistributedCacheEntryOptions GetDistributedCacheEntryOptions(
            TimeSpan? absoluteExpireTime = null,
            TimeSpan? unusedExpireTime = null)
        {
            return new()
            {
                AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromMinutes(1),
                SlidingExpiration = unusedExpireTime
            };
        }
    }
}
