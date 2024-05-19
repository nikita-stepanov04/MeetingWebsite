using MeetingWebsite.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MeetingWebsite.Web.Models.ViewModels
{
    public class RegisterViewModel : LoginViewModel
    {
        [Required]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Username must be from 2 to 30 signs long")]
        [Remote("Username", "Validation", ErrorMessage = "Username already is taken")]
        public override string? Username { get; set; }

        [Required]
        public User UserData { get; set; } = new();

        public IFormFile? Image { get; set; }

        public IEnumerable<long>? CheckInterestsIds { get; set; }
    }
}
