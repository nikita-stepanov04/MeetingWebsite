using Microsoft.AspNetCore.Identity;
using MeetingWebsite.Domain.Models;

namespace MeetingWebsite.Infrastracture.Models.Identity
{
    public class AppRole : IdentityRole<long> 
    {
        public AppRole() : base() {}
        public AppRole(string role) : base(role) {}
    }

    public class AppUser : IdentityUser<long>
    {
        public long UserDataId { get; set; }
        public User? UserData { get; set; }
    }
}
