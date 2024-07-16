using MeetingWebsite.Domain.Models;

namespace MeetingWebsite.Web.Models.ViewModels
{
    public class UserViewModel
    {
        public User User { get; set; } = new();
        public FriendshipInfo FriendshipInfo { get; set; } = new();
    }
}
