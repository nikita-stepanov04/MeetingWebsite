using System.ComponentModel.DataAnnotations;

namespace MeetingWebsite.Web.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Username must be from 2 to 30 signs long")]
        public virtual string? Username { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 6, ErrorMessage = "Password must be from 6 to 30 signs long")]
        public virtual string? Password { get; set; }
    }
}
