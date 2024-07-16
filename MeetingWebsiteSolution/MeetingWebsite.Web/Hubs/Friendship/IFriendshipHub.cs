using MeetingWebsite.Domain.Models;

namespace MeetingWebsite.Web.Hubs.Friendship
{
    public interface IFriendshipHub
    {
        Task UpdateFriendshipStatusResponseAsync(IEnumerable<FriendshipInfo> info);

        Task UpdateFriendshipRequestsCountAsync(int count);

        Task ShowErrorAsync(string error);
    }
}
