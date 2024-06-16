using MeetingWebsite.Application.Interfaces;
using MeetingWebsite.Domain.Interfaces;
using MeetingWebsite.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetingWebsite.Application.Services
{
    public class InterestService : IInterestService
    {
        private readonly IRepository<Interest, long> _interestRepository;
        private readonly IDistributedMeetingCache _cache;

        private readonly string _interestsCachePrefix = "Interests";

        public InterestService(IRepository<Interest, long> rep,
            IDistributedMeetingCache cache)
        {
            _interestRepository = rep;
            _cache = cache;
        }

        public Task<Interest> CreateAsync(Interest interest) =>
            _interestRepository.CreateAsync(interest);

        public async Task<Interest?> FindByIdAsync(long id) =>
            (await GetAllAsync()).Find(i => i.InterestId == id);

        public async Task<IEnumerable<Interest>> FindByIdsAsync(IEnumerable<long> ids) =>
            (await GetAllAsync()).Where(i => ids.Contains(i.InterestId));

        public async Task<List<Interest>> GetAllAsync()
        {
            List<Interest>? interests = await _cache.GetInterestAsync(_interestsCachePrefix);
            if (interests == null || interests.Count == 0)
            {
                interests = await _interestRepository.GetQueryable().ToListAsync();
                interests.ForEach(i => i.Users = null);
                await _cache.SetInterestsAsync(_interestsCachePrefix, interests, TimeSpan.FromHours(1));
            }
            return interests;
        }

        public Task<int> SaveChangesAsync() =>
            _interestRepository.SaveChangesAsync();
    }
}
