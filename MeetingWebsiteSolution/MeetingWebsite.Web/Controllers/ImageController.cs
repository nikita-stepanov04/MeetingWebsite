using MeetingWebsite.Domain.Interfaces;
using MeetingWebsite.Domain.Models;
using MeetingWebsite.Infrastracture.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MeetingWebsite.Web.Controllers
{
    [ApiController]
    [Route("img")]
    [AutoValidateAntiforgeryToken]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;
        private readonly IChatService _chatService;
        private readonly IUserService _userService;
        private readonly UserManager<AppUser> _userManager;

        public ImageController(IImageService imageService,
            IChatService chatService,
            IUserService userService,
            UserManager<AppUser> userManager)
        {
            _imageService = imageService;
            _chatService = chatService;
            _userService = userService;
            _userManager = userManager;
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetImage(long id)
        {
            Image? image = await _imageService.FindByIdAsync(id);
            if (image != null)
            {
                if (image.ChatId != null && !await IsUserChatOwner((Guid)image.ChatId))                
                    return BadRequest();                
                return File(image.Bitmap, image.MimeType!);
            }
            return NotFound();
        }

        [Authorize]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(ImageUploadData data)
        {
            Image image = await _imageService.CreateFromFormFileCompressedOriginalAspectRatio(data.File);
            if (data?.chatId != null && await IsUserChatOwner((Guid)data.chatId))
            {
                image.ChatId = data.chatId;
            }
            await _imageService.SaveChangesAsync();
            return Ok(image.ImageId);
        }


        private async Task<bool> IsUserChatOwner(Guid chatId)
        {
            Chat? chat = await _chatService.GetChatAsync(chatId);
            if (chat != null)
            {
                AppUser? appUser = await _userManager.FindByNameAsync(User.Identity?.Name!);
                if (appUser != null)
                {
                    User? user = await _userService.FindByIdAsync(appUser.UserDataId);
                    if (user != null)
                    {
                        if (chat.User1Id == user.UserId || chat.User2Id == user.UserId)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public class ImageUploadData
        {
            public IFormFile File { get; set; } = null!;
            public Guid? chatId { get; set; }
        }
    }
}
