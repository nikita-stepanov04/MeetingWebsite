using MeetingWebsite.Domain.Interfaces;
using MeetingWebsite.Domain.Models;
using MeetingWebsite.Infrastracture.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MeetingWebsite.Web.Controllers
{
    [Authorize]
    [Route("/chat")]
    public class ChatController : MeetingWebsiteViewController
    {
        private readonly IChatService _chatService;
        private readonly IUserService _userService;
        private readonly UserManager<AppUser> _userManager;

        public ChatController(IChatService chatService,
            UserManager<AppUser> userManager,
            IUserService userService)
        {
            _chatService = chatService;
            _userManager = userManager;
            _userService = userService;
        }

        [HttpGet("forUser/{userId:long}")]
        public async Task<IActionResult> GetChat(long userId)
        {
            AppUser? appUser = await _userManager.FindByNameAsync(User.Identity?.Name!);
            if (appUser != null)
            {
                Chat? chat = await _chatService.GetChatAsync(appUser.UserDataId, userId);
                if (chat == null)
                {
                    chat = await _chatService.CreateChatAsync(appUser.UserDataId, userId);
                    await _chatService.SaveChangesAsync();
                }
                return RedirectToAction(nameof(GetChatById), new { chatId = chat.ChatId });
            }
            return BadRequest();
        }

        [HttpGet("{chatId:guid}")]
        public async Task<IActionResult> GetChatById(Guid chatId)
        {
            AppUser? appUser = await _userManager.FindByNameAsync(User.Identity?.Name!);
            Chat? chat = await _chatService.GetChatAsync(chatId);
            if (chat != null && appUser != null)
            {
                chat.User1 = await _userService.FindByIdAsync(appUser.UserDataId);                
                chat.User2 = await _userService.FindByIdAsync(appUser.UserDataId == chat.User1Id
                    ? chat.User2Id : chat.User1Id);
                chat.User1Id = chat.User1?.UserId ?? default;
                chat.User2Id = chat.User2?.UserId ?? default;
            }
            return View("Chat", chat);
        }
    }
}
