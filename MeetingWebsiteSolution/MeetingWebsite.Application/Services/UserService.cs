using MeetingWebsite.Domain.Interfaces;
using MeetingWebsite.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetingWebsite.Application.Services
{
    public class UserService : IUserService
    {
        private IRepository<User, long> _userRepository;
        public UserService(IRepository<User, long> rep)
        {
            _userRepository = rep;
        }

        public Task<User?> FindByIdAsync(long id) =>
            _userRepository.GetQueryable()
                .Include(u => u.Interests)
                .Include(u => u.Friends)
                .FirstOrDefaultAsync(u => u.UserId == id);

        public Task<User> CreateAsync(User entity) =>
            _userRepository.CreateAsync(entity);

        public ValueTask<User> UpdateAsync(User entity) =>
            _userRepository.UpdateAsync(entity);

        public ValueTask<User> DeleteAsync(User entity) =>
            _userRepository.DeleteAsync(entity);

        public Task<int> SaveChangesAsync() =>
            _userRepository.SaveChangesAsync();

        public async Task<IEnumerable<User>> FindUsersByFiltersAndPagingInfo(
            FilterInfo? filterInfo, PagingInfo pagingInfo, IEnumerable<User> except)
        {
            var exceptIds = except.Select(u => u.UserId);
            var users = _userRepository.GetQueryable()
                .Where(u => !exceptIds.Contains(u.UserId));

            if (filterInfo != null)
            {
                users = users
                    .AgeFilter(filterInfo.AgeMin, filterInfo.AgeMax)
                    .GenderFilter(filterInfo.Genders)
                    .InterestsFilter(filterInfo.CheckInterestsIds);
            }                

            long usersCount = await users.CountAsync();
            pagingInfo.TotalPages = (int)Math.Ceiling(usersCount / (double)pagingInfo.ItemsPerPage);

            return await users.OrderBy(u => u.Firstname)
                .ThenBy(u => u.Secondname)
                .ThenBy(u => u.Birthday)
                .Skip((pagingInfo.CurrentPage - 1) * pagingInfo.ItemsPerPage)
                .Take(pagingInfo.ItemsPerPage)
                .ToListAsync();
        }
    }

    internal static class IQueryableExtentions
    {
        public static IQueryable<User> AgeFilter(this IQueryable<User> query,
            int minAge, int maxAge)
        {
            maxAge = maxAge == 0 ? 99 : maxAge; 
            if (minAge < maxAge)
            {
                DateOnly min = DateOnly.FromDateTime(DateTime.Today.AddYears(-maxAge - 1).AddDays(1));
                DateOnly max = DateOnly.FromDateTime(DateTime.Today.AddYears(-minAge));
                query = query.Where(u => u.Birthday >= min && u.Birthday <= max);
            }
            return query;
        }

        public static IQueryable<User> InterestsFilter(this IQueryable<User> query,
            IEnumerable<long> ids)
        {
            if (ids.Any())
            {
                query = query
                    .Include(u => u.Interests)
                    .Where(u => u.Interests.Any(i => ids.Contains(i.InterestId)));
            }
            return query;
        }

        public static IQueryable<User> GenderFilter(this IQueryable<User> query,
            IEnumerable<UserGender> genders)
        {
            if (genders.Any())
            {
                query = query.Where(u => genders.Contains(u.Gender));
            }
            return query;
        }
    }
}
