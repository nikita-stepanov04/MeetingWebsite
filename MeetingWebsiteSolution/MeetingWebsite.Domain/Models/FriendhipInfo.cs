namespace MeetingWebsite.Domain.Models
{
    public class FriendhipInfo
    {
        public bool AreFriends { get; set; }
        public bool RequestReceived { get; set; }
        public bool RequestSent { get; set; }
        public FriendshipRequest? Request { get; set; }
    }
}
