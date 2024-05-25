using MeetingWebsite.Domain.Interfaces;
using MeetingWebsite.Domain.Models;
using MeetingWebsite.Infrastracture.Models.Identity;
using MeetingWebsite.Web.Helpers;
using MeetingWebsite.Web.Models;
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
        private readonly IInterestService _interestService;
        private readonly IImageService _imageService;
        private readonly SignInManager<AppUser> _singInManager;
        private readonly IConfiguration _config;

        public MeetingController(UserManager<AppUser> userManager,
            IUserService userService,
            IInterestService interestService,
            IImageService imageService,
            SignInManager<AppUser> singInManager,
            IConfiguration config)
        {
            _userManager = userManager;
            _userService = userService;
            _interestService = interestService;
            _imageService = imageService;
            _singInManager = singInManager;
            _config = config;
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

        [HttpGet("user/{id:long}")]
        public async Task<IActionResult> GetUser(long id)
        {
            var user = await _userService.FindByIdAsync(id);
            if (user != null)
            {
                user.ImageLink = Url.GetImageUrl(user);
                return View("User", user);
            }
            return NotFound();
        }

        [HttpGet("my-account")]
        public async Task<IActionResult> MyAccount()
        {
            var appUser = await _userManager.FindByNameAsync(User.Identity?.Name!);
            if (appUser != null)
            {
                appUser.UserData = await _userService.FindByIdAsync(appUser.UserDataId);
                if (appUser.UserData != null)
                {
                    appUser.UserData.ImageLink = Url.GetImageUrl(appUser.UserData);
                    return View(new AccountViewModel()
                    {
                        Username = appUser.UserName,
                        UserData = appUser.UserData,
                        CheckInterestsIds = appUser.UserData.Interests?
                            .Select(i => i.InterestId).ToList()
                    });
                }
            }
            return NotFound();
        }

        [HttpPost("my-account")]
        public async Task<IActionResult> EditAccount(AccountViewModel model)
        {
            ModelState.Remove("Password");
            if (ModelState.IsValid)
            {
                AppUser? appUser = await _userManager.FindByNameAsync(User.Identity?.Name!);
                if (appUser != null)
                {
                    User? user = await _userService.FindByIdAsync(appUser.UserDataId);
                    if (user != null)
                    {
                        user.Firstname = model.UserData.Firstname;
                        user.Secondname = model.UserData.Secondname;
                        user.Gender = model.UserData.Gender;
                        user.Birthday = model.UserData.Birthday;

                        if (model.CheckInterestsIds != null)
                            user.Interests = await _interestService
                                .FindByIdsAsync(model.CheckInterestsIds);
                        if (model.Image != null)
                            user.Image = await _imageService.CreateFromFormFileAsync(model.Image);
                        await _userService.UpdateAsync(user);
                        await _userService.SaveChangesAsync();
                    }
                    if (model.NewPassword != null && model.OldPassword != null)
                    {                       
                        var result = await _userManager.ChangePasswordAsync(appUser,
                            model.OldPassword, model.NewPassword);
                        if (!result.Succeeded)
                        {
                            ModelState.AddModelError("NewPassword", "Failed to update password");
                            return View("MyAccount", model);
                        }                        
                    }
                    if (model.Username != appUser.UserName)
                    {
                        AppUser? checkName = await _userManager.FindByNameAsync(model.Username!);
                        if (checkName == null)
                        {
                            appUser.UserName = model.Username;
                            var result = await _userManager.UpdateAsync(appUser); 
                            if (!result.Succeeded)
                            {
                                ModelState.AddModelError("Username", "Failed to update username");
                                return View("MyAccount", model);
                            }

                            var expires = DateTime.UtcNow.AddHours(2);
                            string token = IdentityServices.GenerateToken(
                                appUser.UserName!, _config["JwtSecret"]!, expires);
                            IdentityServices.SetTokenCookie(token, Response, expires);
                        }    
                        else
                        {
                            ModelState.AddModelError("Username", "Username is alredy taken");
                            return View("MyAccount", model);
                        }                            
                    }                    
                    return RedirectToAction("MyAccount");
                }
                ModelState.AddModelError("Validation", "Failed to authenticate user");
            }            
            return View("MyAccount", model);
        }
    }
}
