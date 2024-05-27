using MeetingWebsite.Domain.Models;

namespace MeetingWebsite.Domain.Interfaces
{
    public interface IFriendshipService
    {
        Task<bool> SendFriendshipRequestAsync(long senderId, long receiverId);

        Task<bool> AcceptFriendshipRequestAsync(long senderId, long receiverId);

        Task<bool> RejectFriendshipRequestAsync(long senderId, long receiverId);

        Task<bool> DeleteFromFriendsAsync(long userId, long friendId);

        Task<IEnumerable<FriendshipRequest>> GetFriendshipRequestsAsync(User receiver);

        Task<int> GetFriendshipRequestsCountAsync(User receiver);

        Task<IEnumerable<FriendshipRequest>> GetSentRequestsAsync(User sender);
    }
}
