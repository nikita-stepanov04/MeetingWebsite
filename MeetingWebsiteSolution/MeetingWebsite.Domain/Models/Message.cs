using System.ComponentModel.DataAnnotations;

namespace MeetingWebsite.Domain.Models
{
    public class Message
    {
        public long MessageId { get; set; }
        public Guid ChatId { get; set; }

        [StringLength(1024, MinimumLength = 1)]
        public string? Text { get; set; }
        public long? ImageId { get; set; }
        public long AuthorId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }

        public string? ImageLink { get; set; }
    }
}
