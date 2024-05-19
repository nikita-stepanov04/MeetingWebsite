using System.ComponentModel.DataAnnotations;

namespace MeetingWebsite.Domain.Models
{
    public class Image
    {
        public long ImageId { get; set; }

        [Required, MinLength(1)]
        public byte[] Bitmap { get; set; } = [];

        [Required]
        public string? MimeType { get; set; }
    }
}
