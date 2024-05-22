using MeetingWebsite.Domain.Interfaces;
using MeetingWebsite.Infrastracture.Models.Identity;
using MeetingWebsite.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MeetingWebsite.Web.Controllers
{
    [Route("/account")]
    [AutoValidateAntiforgeryToken]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _singInManager;
        private readonly IUserService _userDataService;
        private readonly IInterestService _interestService;
        private readonly IImageService _imageService;
        private readonly IConfiguration _config;

        public AccountController(UserManager<AppUser> userManager,
            SignInManager<AppUser> singInManager,
            IConfiguration config,
            IUserService userService,
            IInterestService interestService,
            IImageService imageService)
        {
            _userManager = userManager;
            _singInManager = singInManager;
            _config = config;
            _userDataService = userService;
            _interestService = interestService;
            _imageService = imageService;
        }

        //// action for registration testing
        //[Authorize]
        //[HttpGet("index")]
        //public async Task<IActionResult> Index()
        //{
        //    var user = await _userManager.FindByNameAsync(User.Identity.Name);
        //    user.UserData = await _userDataService.FindByIdAsync(user.UserDataId);
        //    user.UserData.ImageLink = Url.Action("GetImage", "Image", new { id = user.UserData.ImageId });
        //    return View(user);
        //}

        [HttpGet("login")]
        public IActionResult Login() => View();

        [HttpPost("login")]
        public async Task<IActionResult> LoginPost(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await CheckPassword(model))
                {
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSecret"]!));
                    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var expires = DateTime.UtcNow.AddHours(2);

                    var token = new JwtSecurityToken(
                        claims: [new Claim(ClaimTypes.Name, model.Username!)],
                        expires: expires,
                        signingCredentials: credentials
                    );

                    var handler = new JwtSecurityTokenHandler();
                    Response.Cookies.Append("Bearer", handler.WriteToken(token),
                        new CookieOptions()
                        {
                            Expires = expires,
                            Secure = true,
                            IsEssential = true
                        });
                    return RedirectToAction("Index", "Meeting");
                }
                else
                {
                    ModelState.AddModelError("Password", "Incorrect login or password");
                }
            }
            return View("Login");
        }

        [HttpGet("register")]
        public IActionResult Register() => View();

        [HttpPost("register")]
        public async Task<IActionResult> RegisterPost(RegisterViewModel model)
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
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await _singInManager.SignOutAsync();
            Response.Cookies.Delete("Bearer");
            return RedirectToAction("Login");
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
