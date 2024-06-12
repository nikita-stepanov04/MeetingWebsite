using MeetingWebsite.Application.Interfaces;
using MeetingWebsite.Domain.Models;
using MeetingWebsite.Infrastracture.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

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

        public async Task SetRecordAsync<T, TId>(string prefix, TId id, T value,
            TimeSpan? absoluteExpireTime = null,
            TimeSpan? unusedExpireTime = null) where T : class
        {
            var options = GetDistributedCacheEntryOptions(absoluteExpireTime, unusedExpireTime);
            string json = JsonConvert.SerializeObject(value,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            await _cache.SetStringAsync(GetCacheKey(prefix, id), json, options);
        }

        public async Task<T?> GetRecordAsync<T, TId>(string prefix, TId id) where T : class
        {
            T? local = _context.Set<T>().Local.FindEntry(id)?.Entity;
            if (local == null)
            {
                string? json = await _cache.GetStringAsync(GetCacheKey(prefix, id));
                if (json != null)
                {
                    T entity = JsonConvert.DeserializeObject<T>(json)!;
                    _context.Set<T>().Attach(entity);
                    return entity;
                }
            }
            return local;
        }

        public async Task<User?> GetUserAsync(string prefix, long id)
        {
            User? local = _context.Users.Local.FindEntry(id)?.Entity;
            if (local == null)
            {
                string? json = await _cache.GetStringAsync(GetCacheKey(prefix, id));
                if (json != null)
                {
                    User user = JsonConvert.DeserializeObject<User>(json)!;

                    var friendsIds = user.Friends?.Select(f => f.UserId) ?? [];
                    var interestsIds = user.Interests?.Select(f => f.InterestId) ?? [];

                    foreach (User friend in _context.Users.Local
                        .Where(u => friendsIds.Contains(u.UserId)))
                    {
                        _context.Users.Local.Remove(friend);
                    }
                    foreach (Interest interest in _context.Interests.Local
                        .Where(u => interestsIds.Contains(u.InterestId)))
                    {
                        _context.Interests.Local.Remove(interest);
                    }
                    foreach (Interest interest in user.Interests ?? [])
                    {
                        interest.Users = null;
                    }

                    _context.Users.Attach(user);
                    return user;
                }
            }
            return local;
        }

        public Task RemoveRecordAsync<TId>(string prefix, TId id) =>
            _cache.RemoveAsync(GetCacheKey(prefix, id));

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

        private string GetCacheKey<TId>(string prefix, TId id) => $"{prefix}_{id}";
    }
}
