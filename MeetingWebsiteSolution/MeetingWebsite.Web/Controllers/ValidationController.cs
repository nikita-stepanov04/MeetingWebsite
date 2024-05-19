using MeetingWebsite.Infrastracture.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MeetingWebsite.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ValidationController : ControllerBase
    {
        private UserManager<AppUser> _userManager;

        public ValidationController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("username")]
        public async Task<bool> Username(string username)
        {
            AppUser? user = await _userManager.FindByNameAsync(username);            
            return user == null ? true : false;
        }
    }
}
