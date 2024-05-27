using MeetingWebsite.Domain.Interfaces;
using MeetingWebsite.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetingWebsite.Application.Services
{
    public class FriendshipService : IFriendshipService
    {
        private readonly IRepository<FriendshipRequest, long> _repository;
        private readonly IRepository<User, long> _userRepository;

        public FriendshipService(IRepository<FriendshipRequest, long> repository,
            IRepository<User, long> userRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
        }

        public async Task<bool> SendFriendshipRequestAsync(long senderId, long receiverId)
        {
            User? sender = await FindByIdAsync(senderId);
            User? receiver = await FindByIdAsync(receiverId);

            if (sender != null && receiver != null)
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
                List<User> userFriends = user.Friends as List<User> ?? new();
                List<User> friendFriends = friend.Friends as List<User> ?? new();

                userFriends.Remove(friend);
                friendFriends.Remove(user);

                user.Friends = userFriends;
                friend.Friends = friendFriends;

                await _userRepository.UpdateAsync(user);
                await _userRepository.UpdateAsync(friend);
                await _repository.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> AcceptFriendshipRequestAsync(long senderId, long receiverId)
        {
            FriendshipRequest? request = await GetFriendshipRequest(senderId, receiverId);
            if (request != null)
            {
                List<User> senderFriends = request.Sender.Friends as List<User> ?? new();
                List<User> receiverFriends = request.Receiver.Friends as List<User> ?? new();

                senderFriends.Add(request.Receiver);
                receiverFriends.Add(request.Sender);

                request.Sender.Friends = senderFriends;
                request.Receiver.Friends = receiverFriends;

                await _userRepository.UpdateAsync(request.Sender);
                await _userRepository.UpdateAsync(request.Receiver);

                await _repository.DeleteAsync(request);
                await _repository.SaveChangesAsync();
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
