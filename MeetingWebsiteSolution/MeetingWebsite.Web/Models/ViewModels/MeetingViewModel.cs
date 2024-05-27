using MeetingWebsite.Domain.Models;

namespace MeetingWebsite.Web.Models.ViewModels
{
    public class MeetingViewModel
    {
        public IEnumerable<User> Users { get; set; } = Enumerable.Empty<User>();
        public PagingInfo PagInfo { get; set; } = new();
        public FilterInfo? FilterInfo { get; set; }

        public HashSet<long> FriendshipRequestSendersIds { get; set; } = new();
        public HashSet<long> SentFriendshipRequestsReceiversIds { get; set; } = new();
        public HashSet<long> FriendsIds { get; set; } = new();
    }
}
