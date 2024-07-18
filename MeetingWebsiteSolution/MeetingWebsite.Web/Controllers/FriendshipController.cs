using MeetingWebsite.Domain.Interfaces;
using MeetingWebsite.Domain.Models;
using MeetingWebsite.Infrastracture.Models.Identity;
using MeetingWebsite.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MeetingWebsite.Web.Controllers
{
    [Authorize]
    [Route("/api/")]
    public class FriendshipController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IFriendshipService _friendshipService;
        private readonly UserManager<AppUser> _userManager;

        public FriendshipController(IUserService userService,
            UserManager<AppUser> userManager,
            IFriendshipService friendshipService)
        {
            _userService = userService;
            _userManager = userManager;
            _friendshipService = friendshipService;
        }

        public Task<AppUser> AppUserTask => _userManager.FindByNameAsync(User.Identity?.Name!)!;

        [HttpGet("user/{id:long}")]
        public async Task<IActionResult> GetUserJson(long id)
        {
            var friend = await _userService.FindByIdAsync(id);
            if (friend != null)
            {
                friend.Friends = null;
                friend.Interests = null;
                friend.ImageLink = Url.GetImageUrl(friend);
                return Ok(friend);
            }
            return NotFound();
        }

        [HttpGet("friendship-requests")]
        public async Task<IActionResult> GetFriendshipRequests()
        {
            User user = (await _userService.FindByIdAsync((await AppUserTask).UserDataId))!;
            var requests = await _friendshipService.GetFriendshipRequestsAsync(user);
            foreach (var request in requests)
            {
                request.Receiver = null;
                request.Sender.Interests = null;
                request.Sender.Friends = null;
                request.Sender.ImageLink = Url.GetImageUrl(request.Sender);
            }
            return Ok(requests);
        }
    }
}
