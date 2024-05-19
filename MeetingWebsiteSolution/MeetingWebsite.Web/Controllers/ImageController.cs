using MeetingWebsite.Domain.Interfaces;
using MeetingWebsite.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace MeetingWebsite.Web.Controllers
{
    [ApiController]
    [Route("img")]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;

        public ImageController(IImageService imageService)
        {
            _imageService = imageService;
        }

        [Route("{id:long}")]
        public async Task<IActionResult> GetImage(long id)
        {
            Image? image = await _imageService.FindByIdAsync(id);
            if (image != null)
            {
                return File(image.Bitmap, image.MimeType!);
            }
            return NotFound();
        }
    }
}
