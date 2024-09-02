using MeetingWebsite.Application.Interfaces;
using MeetingWebsite.Domain.Interfaces;
using MeetingWebsite.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetingWebsite.Application.Services
{
    public class FriendshipService : IFriendshipService
    {
        private readonly IRepository<FriendshipRequest, long> _repository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IDistributedMeetingCache _cache;

        private readonly string _userCachePrefix = "User";

        public FriendshipService(IRepository<FriendshipRequest, long> repository,
            IRepository<User, long> userRepository,
            IDistributedMeetingCache cache)
        {
            _repository = repository;
            _userRepository = userRepository;
            _cache = cache;
        }

        public async Task<bool> SendFriendshipRequestAsync(long senderId, long receiverId)
        {
            User? sender = await FindByIdAsync(senderId);
            User? receiver = await FindByIdAsync(receiverId);

            if (sender != null && receiver != null
                && (await GetFriendshipRequest(receiverId, senderId)) == null)
            {
                await _repository.CreateAsync(new FriendshipRequest()
                {
                    Sender = sender,
                    Receiver = receiver
                });
                await _repository.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteFromFriendsAsync(long userId, long friendId)
        {
            User? user = await FindByIdAsync(userId);
            User? friend = await FindByIdAsync(friendId);
            if (user != null && friend != null)
            {
                HashSet<User> userFriends = user.Friends?.ToHashSet() ?? new();
                HashSet<User> friendFriends = friend.Friends?.ToHashSet() ?? new();

                userFriends.Remove(friend);
                friendFriends.Remove(user);

                user.Friends = userFriends;
                friend.Friends = friendFriends;

                await _userRepository.UpdateAsync(user);
                await _userRepository.UpdateAsync(friend);
                await _repository.SaveChangesAsync();

                await _cache.RemoveRecordAsync(_userCachePrefix, userId);
                await _cache.RemoveRecordAsync(_userCachePrefix, friendId);
                return true;
            }
            return false;
        }

        public async Task<bool> AcceptFriendshipRequestAsync(long senderId, long receiverId)
        {
            FriendshipRequest? request = await GetFriendshipRequest(senderId, receiverId);
            User? sender = await FindByIdAsync(senderId);
            User? receiver = await FindByIdAsync(receiverId);
            if (request != null && sender != null && receiver != null)
            {
                HashSet<User> senderFriends = sender.Friends?.ToHashSet() ?? new();
                HashSet<User> receiverFriends = receiver.Friends?.ToHashSet() ?? new();

                senderFriends.Add(request.Receiver);
                receiverFriends.Add(request.Sender);

                sender.Friends = senderFriends;
                receiver.Friends = receiverFriends;

                await _userRepository.UpdateAsync(sender);
                await _userRepository.UpdateAsync(receiver);

                await _repository.DeleteAsync(request);
                await _repository.SaveChangesAsync();

                await _cache.RemoveRecordAsync(_userCachePrefix, senderId);
                await _cache.RemoveRecordAsync(_userCachePrefix, receiverId);
                return true;
            }
            return false;
        }

        public async Task<bool> RejectFriendshipRequestAsync(long senderId, long receiverId)
        {
            FriendshipRequest? request = await GetFriendshipRequest(senderId, receiverId);
            if (request != null)
            {
                await _repository.DeleteAsync(request);
                await _repository.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task RejectFriendshipRequestsAsync(IEnumerable<FriendshipRequest> requests)
        {        
            foreach(var req in requests)
            {
                await _repository.DeleteAsync(req);
            }            
            await _repository.SaveChangesAsync();
        }

        public async Task<IEnumerable<FriendshipRequest>> GetFriendshipRequestsAsync(User receiver) =>
            await _repository.GetQueryable()
                .Include(fr => fr.Sender)
                .Where(fr => fr.ReceiverId == receiver.UserId)
                .ToListAsync();

        public Task<int> GetFriendshipRequestsCountAsync(User receiver) =>
            _repository.GetQueryable()
                .Include(fr => fr.Sender)
                .Where(fr => fr.ReceiverId == receiver.UserId)
                .CountAsync();

        public async Task<IEnumerable<FriendshipRequest>> GetSentRequestsAsync(User sender) =>
            await _repository.GetQueryable()
                .Include(fr => fr.Receiver)
                .Where(fr => fr.SenderId == sender.UserId)
                .ToListAsync();

        public async Task<IEnumerable<FriendshipInfo>> GetFriendshipStatus(
            long userId, params long[] ids)
        {
            var friendshipInfos = new List<FriendshipInfo>();
            foreach (long id in ids)
            {
                var friendshipInfo = await GetFriendhipInfo(userId, id);
                friendshipInfos.Add(friendshipInfo);
            }
            return friendshipInfos;
        }


        private async Task<FriendshipInfo> GetFriendhipInfo(long userId, long friendId)
        {
            User? user = await FindByIdAsync(userId);
            User? friend = await FindByIdAsync(friendId);

            FriendshipStatus status;
            if (user != null && friend != null)
            {                
                if (user.Friends?.Select(u => u.UserId).Contains(friend.UserId) == false)
                {
                    FriendshipRequest? request = await GetNonPopulatedFriendshipRequest(
                        user.UserId, friend.UserId);
                    if (request != null)
                        status = FriendshipStatus.RequestSent;
                    else
                    {
                        request = await GetNonPopulatedFriendshipRequest(friend.UserId, user.UserId);
                        if (request != null)
                            status = FriendshipStatus.PendingAcceptance;
                        else
                            status = FriendshipStatus.NotAFriend;
                    }
                }
                else
                    status = FriendshipStatus.Friend;
            }
            else
                status = FriendshipStatus.NotAFriend;
            return new FriendshipInfo
            {
                UserId = friendId,
                FriendshipStatus = status
            };
        }

        private async Task<FriendshipRequest?> GetNonPopulatedFriendshipRequest(
            long senderId, long receiverId)
        {
            return await _repository.GetQueryable()
                .FirstOrDefaultAsync(fr => fr.SenderId == senderId
                    && fr.ReceiverId == receiverId);
        }

        private async Task<FriendshipRequest?> GetFriendshipRequest(long senderId, long receiverId) =>
            await _repository.GetQueryable()
                .Include(fr => fr.Sender).ThenInclude(s => s.Friends)
                .Include(fr => fr.Receiver).ThenInclude(r => r.Friends)
                .FirstOrDefaultAsync(fr => fr.SenderId == senderId && fr.ReceiverId == receiverId);

        private Task<User?> FindByIdAsync(long id) =>
            _userRepository.GetQueryable()
                .Include(u => u.Friends)
                .FirstOrDefaultAsync(u => u.UserId == id);
    }
}
