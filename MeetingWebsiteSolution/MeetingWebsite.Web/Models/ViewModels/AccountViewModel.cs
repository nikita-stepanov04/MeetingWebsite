using MeetingWebsite.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MeetingWebsite.Web.Models.ViewModels
{
    public class AccountViewModel : LoginViewModel
    {
        [Remote("Username", "Validation", ErrorMessage = "Username is already taken")]
        public override string? Username { get; set; }

        [StringLength(30, MinimumLength = 6, ErrorMessage = "Password must be from 6 to 30 signs long")]
        public string? NewPassword { get; set; }

        [StringLength(30, MinimumLength = 6, ErrorMessage = "Password must be from 6 to 30 signs long")]
        public string? OldPassword { get; set; }

        [Required]
        public virtual User UserData { get; set; } = new();

        public IFormFile? Image { get; set; }

        public IEnumerable<long>? CheckInterestsIds { get; set; }
    }
}
