﻿using MeetingWebsite.Application.Interfaces;
using MeetingWebsite.Domain.Interfaces;
using MeetingWebsite.Domain.Models;
using Microsoft.EntityFrameworkCore;
using static System.Math;

namespace MeetingWebsite.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Chat, Guid> _chatRepository;
        private readonly IFriendshipService _frindshipService;
        private readonly IImageService _imageService;
        private readonly IDistributedMeetingCache _cache;
        private readonly string UserCachePrefix = "User";

        public UserService(IRepository<User, long> rep,
            IImageService imageService,
            IDistributedMeetingCache cache,
            IRepository<Chat, Guid> chatRepository,
            IFriendshipService frindshipService)
        {
            _userRepository = rep;
            _imageService = imageService;
            _cache = cache;
            _chatRepository = chatRepository;
            _frindshipService = frindshipService;
        }

        public async Task<User?> FindByIdAsync(long id)
        {
            User? user =  await _cache.GetUserAsync(UserCachePrefix, id);
            if (user == null)
            {
                user = await _userRepository.GetQueryable()
                    .Include(u => u.Interests)                        
                    .Include(u => u.Friends)
                    .FirstOrDefaultAsync(u => u.UserId == id);
                if (user != null)
                {
                    foreach (Interest interest in user.Interests ?? [])
                    {
                        interest.Users = null;
                    }
                    foreach (User friend in user.Friends ?? [])
                    {
                        friend.Friends = null;
                        friend.Interests = null;
                    }
                    await _cache.SetRecordAsync(UserCachePrefix, id, user);
                }
            }
            return user;
        }

        public Task<User> CreateAsync(User entity) =>
            _userRepository.CreateAsync(entity);

        public ValueTask<User> UpdateAsync(User entity)
        {
            _cache.RemoveRecordAsync(UserCachePrefix, entity.UserId);
            return _userRepository.UpdateAsync(entity);
        }

        public async Task DeleteAsync(User entity)
        {
            List<Chat> userChats = await _chatRepository.GetQueryable()
                .Where(u => u.User1Id == entity.UserId || u.User2Id == entity.UserId)
                .ToListAsync();
            userChats.ForEach(async c => await _chatRepository.DeleteAsync(c));

            Image? image = await _imageService.FindByIdAsync((long)entity.ImageId!);
            if (image != null)
            {
                await _imageService.Remove(image);
            }

            var friendshipRequests = await _frindshipService.GetFriendshipRequestsAsync(entity);
            var sentfriendshipRequests = await _frindshipService.GetSentRequestsAsync(entity);
            await _frindshipService.RejectFriendshipRequestsAsync(
                friendshipRequests.Concat(sentfriendshipRequests));

            await _cache.RemoveRecordAsync(UserCachePrefix, entity.UserId);
            await _userRepository.DeleteAsync(entity);
        }

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
            pagingInfo.TotalPages = (int)Ceiling(usersCount / (double)pagingInfo.ItemsPerPage);
            pagingInfo.CurrentPage = Min(pagingInfo.CurrentPage, pagingInfo.TotalPages);

            return await users
                .OrderBy(u => u.Firstname)
                .ThenBy(u => u.Secondname)
                .ThenBy(u => u.Birthday)
                .Skip((pagingInfo.CurrentPage - 1) * pagingInfo.ItemsPerPage)
                .Take(pagingInfo.ItemsPerPage)
                .ToListAsync();
        }

        public async Task<int> Count() => await _userRepository.GetQueryable().CountAsync();
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
