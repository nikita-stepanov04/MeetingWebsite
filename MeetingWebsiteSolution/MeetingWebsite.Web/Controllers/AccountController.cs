using MeetingWebsite.Domain.Interfaces;
using MeetingWebsite.Domain.Models;
using MeetingWebsite.Infrastracture.Models.Identity;
using MeetingWebsite.Web.Filters;
using MeetingWebsite.Web.Helpers;
using MeetingWebsite.Web.Models;
using MeetingWebsite.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MeetingWebsite.Web.Controllers
{
    [Route("/account")]
    [AutoValidateAntiforgeryToken]
    public class AccountController : MeetingWebsiteViewController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _singInManager;
        private readonly IUserService _userDataService;
        private readonly IInterestService _interestService;
        private readonly IImageService _imageService;
        private readonly IConfiguration _config;
        private readonly IFriendshipService _friendshipService;

        public AccountController(UserManager<AppUser> userManager,
            SignInManager<AppUser> singInManager,
            IConfiguration config,
            IUserService userService,
            IInterestService interestService,
            IImageService imageService,
            IFriendshipService friendshipService)
        {
            _userManager = userManager;
            _singInManager = singInManager;
            _config = config;
            _userDataService = userService;
            _interestService = interestService;
            _imageService = imageService;
            _friendshipService = friendshipService;
        }

        [HttpGet("login")]
        public IActionResult Login() => View();

        [HttpPost("login")]
        public async Task<IActionResult> LoginPost(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await CheckPassword(model))
                {
                    var expires = DateTime.UtcNow.AddHours(2);
                    string token = IdentityServices.GenerateToken(
                        model.Username!, _config["JwtSecret"]!, expires);
                    IdentityServices.SetTokenCookie(token, Response, expires);
                    return RedirectToAction("Index", "Meeting");
                }
                else
                    ModelState.AddModelError("Password", "Incorrect login or password");
            }
            return View("Login");
        }

        [HttpGet("register")]
        public IActionResult Register() => View();

        [HttpPost("register")]
        public async Task<IActionResult> RegisterPost(AccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.CheckInterestsIds != null)
                    model.UserData.Interests = await _interestService
                        .FindByIdsAsync(model.CheckInterestsIds);
                if (model.Image != null)
                    model.UserData.Image = await _imageService.CreateFromFormFileAsync(model.Image);

                await _userDataService.CreateAsync(model.UserData);
                await _userDataService.SaveChangesAsync();

                var user = new AppUser()
                {
                    UserName = model.Username,
                    UserDataId = model.UserData.UserId
                };
                var result = await _userManager.CreateAsync(user, model.Password!);

                if (result.Succeeded)
                {
                    return RedirectToAction("Login");
                }
                ModelState.AddModelError("", "Something went wrong");
            }
            return View("Register");
        }

        [Authorize]
        [HttpGet("/my-account")]
        public async Task<IActionResult> MyAccount()
        {
            var appUser = await _userManager.FindByNameAsync(User.Identity?.Name!);
            if (appUser != null)
            {
                appUser.UserData = await _userDataService.FindByIdAsync(appUser.UserDataId);
                if (appUser.UserData != null)
                {
                    var requests = await _friendshipService
                        .GetFriendshipRequestsAsync(appUser.UserData);
                    return View(new AccountViewModel()
                    {
                        Username = appUser.UserName,
                        UserData = appUser.UserData,
                        Senders = requests.Select(r => r.Sender),
                        CheckInterestsIds = appUser.UserData.Interests?
                            .Select(i => i.InterestId).ToList()
                    });
                }
            }
            return NotFound();
        }

        [Authorize]
        [HttpPost("/my-account")]
        public async Task<IActionResult> EditAccount(AccountViewModel model)
        {
            ModelState.Remove("Password");
            if (ModelState.IsValid)
            {
                AppUser? appUser = await _userManager.FindByNameAsync(User.Identity?.Name!);
                if (appUser != null)
                {
                    User? user = await _userDataService.FindByIdAsync(appUser.UserDataId);
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
                        {
                            user.Image = user.ImageId == null
                                ? new()
                                : await _imageService.FindByIdAsync((long)user.ImageId);                            
                            await _imageService.UpdateFromFormFileAsync(user.Image!, model.Image);
                        }                            
                        await _userDataService.UpdateAsync(user);
                        await _userDataService.SaveChangesAsync();
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

        [Authorize]
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await _singInManager.SignOutAsync();
            Response.Cookies.Delete("Bearer");
            return RedirectToAction("Login");
        }

        [Authorize]
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteAccount()
        {
            AppUser? appUser = await _userManager.FindByNameAsync(User.Identity?.Name!);
            if (appUser != null)
            {
                User? user = await _userDataService.FindByIdAsync(appUser.UserDataId);
                if (user != null)
                {
                    await _userDataService.DeleteAsync(user);
                    await _userDataService.SaveChangesAsync();
                }
                await _userManager.DeleteAsync(appUser);
                await _singInManager.SignOutAsync();
                Response.Cookies.Delete("Bearer");
                return RedirectToAction("Login");
            }
            return BadRequest();
        }

        private async Task<bool> CheckPassword(LoginViewModel model)
        {
            AppUser? user = await _userManager.FindByNameAsync(model.Username!);
            if (user != null)
            {
                return (await _singInManager.CheckPasswordSignInAsync(
                    user, model.Password!, false)).Succeeded;
            }
            return false;
        }
    }
}