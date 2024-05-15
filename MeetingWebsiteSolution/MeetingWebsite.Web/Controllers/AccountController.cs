using MeetingWebsite.Domain.Models;
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
    [Route("/")]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _singInManager;
        private readonly IConfiguration _config;

        public AccountController(UserManager<AppUser> userManager,
            SignInManager<AppUser> singInManager,
            IConfiguration config)
        {
            _userManager = userManager;
            _singInManager = singInManager;
            _config = config;
        }

        [Authorize]
        [HttpGet("index")]
        public IActionResult Index() => View();

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

                    var token = new JwtSecurityToken(
                        claims: [new Claim(ClaimTypes.Name, model.Username)],
                        expires: DateTime.UtcNow.AddHours(2),
                        signingCredentials: credentials
                    );

                    var handler = new JwtSecurityTokenHandler();
                    Response.Cookies.Append("Bearer", handler.WriteToken(token));
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("Password", "Incorrect login or password");
                }                
            }
            return View("Login");
        }

        private async Task<bool> CheckPassword(LoginViewModel model)
        {
            AppUser? user = await _userManager.FindByNameAsync(model.Username);
            if (user != null)
            {
                return (await _singInManager.CheckPasswordSignInAsync(
                    user, model.Password, false)).Succeeded;
            }
            return false;
        }
    }    
}
