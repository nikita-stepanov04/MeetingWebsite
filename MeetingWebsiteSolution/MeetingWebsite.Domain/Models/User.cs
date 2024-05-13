using System.ComponentModel.DataAnnotations;

namespace MeetingWebsite.Domain.Models
{
    public class User
    {
        public long UserId { get; set; }

        [Required]
        [Length(2, 30)]
        public string Firstname { get; set; } = string.Empty;

        [Required]
        [Length(2, 30)]
        public string Secondname { get; set; } = string.Empty;

        public UserGender Gender { get; set; }
        public DateOnly Birthday { get; set; }
        public IEnumerable<User>? Friends { get; set; }
        public IEnumerable<Interest>? Interests { get; set; }
    }

    public enum UserGender
    {
        Undefined,
        Male,
        Female
    }

    public class Interest
    {
        public long InterestId { get; set; }

        [Required]
        [Length(3, 10)]
        public string InterestType { get; set; } = string.Empty;
        public IEnumerable<User>? Users { get; set; }
    }
}
