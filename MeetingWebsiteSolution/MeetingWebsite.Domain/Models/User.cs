using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeetingWebsite.Domain.Models
{
    public class User
    {
        public long UserId { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Firstname must be from 2 to 30 signs long")]
        public string Firstname { get; set; } = string.Empty;

        [Required]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Secondname must be from 2 to 30 signs long")]
        public string Secondname { get; set; } = string.Empty;

        [Column(TypeName = "varchar(10)")]
        public UserGender Gender { get; set; }
        public DateOnly Birthday { get; set; }
        public IEnumerable<User>? Friends { get; set; }
        public IEnumerable<Interest>? Interests { get; set; }

        public long? ImageId { get; set; }
        public Image? Image { get; set; }

        public string? ImageLink { get; set; }
    }
}
