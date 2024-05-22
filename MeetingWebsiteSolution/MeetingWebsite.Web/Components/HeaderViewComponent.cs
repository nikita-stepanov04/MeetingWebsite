using MeetingWebsite.Application.Services;
using MeetingWebsite.Domain.Interfaces;
using MeetingWebsite.Domain.Models;
using MeetingWebsite.Infrastracture.Models.Identity;
using MeetingWebsite.Web.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace MeetingWebsite.Web.Components
{
    public class HeaderViewComponent : ViewComponent
    {
        private readonly IUrlHelperFactory _helperFactory;
        private readonly IUserService _userService;
        private readonly UserManager<AppUser> _userManager;

        public HeaderViewComponent(IUrlHelperFactory helperFactory,
            IUserService userService, UserManager<AppUser> userManager)
        {
            _helperFactory = helperFactory;
            _userService = userService;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            AppUser? appUser = await _userManager.FindByNameAsync(User.Identity?.Name!);
            if (appUser != null)
            {
                User? user = await _userService.FindByIdAsync(appUser.UserDataId);
                var helper = _helperFactory.GetUrlHelper(ViewContext);
                return View("Default", helper.GetImageUrl(user));              
            }
            return View();
        }
    }
}
