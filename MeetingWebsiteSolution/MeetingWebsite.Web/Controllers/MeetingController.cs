using MeetingWebsite.Domain.Interfaces;
using MeetingWebsite.Domain.Models;
using MeetingWebsite.Infrastracture.Models.Identity;
using MeetingWebsite.Web.Helpers;
using MeetingWebsite.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MeetingWebsite.Web.Controllers
{
    [Authorize]
    [Route("/")]
    public class MeetingController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserService _userService;

        public MeetingController(UserManager<AppUser> userManager, IUserService userService)
        {
            _userManager = userManager;
            _userService = userService;
        }

        [HttpGet()]
        [HttpGet("page/{page:int?}")]
        public async Task<IActionResult> Index(MeetingViewModel? model, int? page)
        {
            model = model ?? new();
            model.PagInfo.CurrentPage = page ?? model.PagInfo.CurrentPage;
            model.Users = await _userService.FindUsersByFiltersAndPagingInfo(
                model.FilterInfo, model.PagInfo);
            foreach (User user in model.Users)
            {
                user.ImageLink = Url.GetImageUrl(user);
            }
            return View(model);
        }
    }
}
