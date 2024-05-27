using System.ComponentModel.DataAnnotations;

namespace MeetingWebsite.Domain.Models
{
    public class FriendshipRequest
    {
        [Key]
        public long RequestId { get; set; }

        [Required]
        public User Sender { get; set; } = new();
        public long SenderId { get; set; }

        [Required]
        public User Receiver { get; set; } = new();
        public long ReceiverId { get; set; }
    }
}
