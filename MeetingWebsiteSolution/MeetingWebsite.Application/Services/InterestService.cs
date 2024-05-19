using MeetingWebsite.Domain.Interfaces;
using MeetingWebsite.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetingWebsite.Application.Services
{
    public class InterestService : IInterestService
    {
        private IRepository<Interest, long> _interestRepository;
        public InterestService(IRepository<Interest, long> rep)
        {
            _interestRepository = rep;
        }

        public Task<Interest> CreateAsync(Interest interest) =>
            _interestRepository.CreateAsync(interest);

        public ValueTask<Interest?> FindByIdAsync(long id) =>
            _interestRepository.FindByIdAsync(id);

        public Task<List<Interest>> FindByIdsAsync(IEnumerable<long> ids) =>
            _interestRepository.GetQueryable()
                .Where(i => ids.Contains(i.InterestId))
                .ToListAsync();

        public Task<List<Interest>> GetAllAsync() =>
            _interestRepository.GetQueryable().ToListAsync();

        public Task<int> SaveChangesAsync() =>
            _interestRepository.SaveChangesAsync();
    }
}
