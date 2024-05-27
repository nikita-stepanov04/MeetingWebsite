using MeetingWebsite.Domain.Interfaces;
using MeetingWebsite.Domain.Models;
using MeetingWebsite.Infrastracture.Models.Identity;
using MeetingWebsite.Web.Helpers;
using MeetingWebsite.Web.Models.ViewModels;
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
        private readonly IFriendshipService _friendshipService;

        public HeaderViewComponent(IUrlHelperFactory helperFactory,
            IUserService userService, UserManager<AppUser> userManager,
            IFriendshipService friendshipService)
        {
            _helperFactory = helperFactory;
            _userService = userService;
            _userManager = userManager;
            _friendshipService = friendshipService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            AppUser? appUser = await _userManager.FindByNameAsync(User.Identity?.Name!);
            if (appUser != null)
            {
                User? user = await _userService.FindByIdAsync(appUser.UserDataId);
                var helper = _helperFactory.GetUrlHelper(ViewContext);

                HeaderViewModel model = new();
                if (user != null)
                {
                    model.ImageUrl = helper.GetImageUrl(user);
                    model.FriendshipRequestsNumber = await _friendshipService
                        .GetFriendshipRequestsCountAsync(user);
                }
                return View("Default", model);
            }
            return View();
        }
    }
}
