using MeetingWebsite.Domain.Interfaces;
using MeetingWebsite.Domain.Models;
using MeetingWebsite.Infrastracture.Models.Identity;
using MeetingWebsite.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MeetingWebsite.Web.Controllers
{
    [Authorize]
    [Route("/")]
    [AutoValidateAntiforgeryToken]
    public class MeetingController : MeetingWebsiteViewController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserService _userService;
        private readonly IFriendshipService _friendshipService;

        public MeetingController(UserManager<AppUser> userManager,
            IUserService userService,
            IFriendshipService friendshipService)
        {
            _userManager = userManager;
            _userService = userService;
            _friendshipService = friendshipService;
        }

        public Task<AppUser> AppUserTask => _userManager.FindByNameAsync(User.Identity?.Name!)!;

        [HttpGet()]
        [HttpGet("page/{page:int?}")]
        public async Task<IActionResult> Index(MeetingViewModel? model, int? page)
        {
            model = model ?? new();
            model.PagInfo.CurrentPage = page ?? model.PagInfo.CurrentPage;

            User? user = await _userService.FindByIdAsync((await AppUserTask).UserDataId);
            if (user != null)
            {
                model.Users = await _userService
                    .FindUsersByFiltersAndPagingInfo(model.FilterInfo, model.PagInfo, [user]);
            }
            return View(model);
        }

        [HttpGet("user/{id:long}")]
        public async Task<IActionResult> GetUser(long id)
        {
            var friend = await _userService.FindByIdAsync(id);
            if (friend != null)
            {
                return View("User", new UserViewModel()
                {
                    User = friend
                });
            }
            return NotFound();
        }
    }
}
