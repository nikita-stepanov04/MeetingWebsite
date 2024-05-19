using System.ComponentModel.DataAnnotations;

namespace MeetingWebsite.Domain.Models
{
    public class Interest
    {
        public long InterestId { get; set; }

        [Required]
        [StringLength(10, MinimumLength = 3)]
        public string InterestType { get; set; } = string.Empty;
        public IEnumerable<User>? Users { get; set; }
    }
}
