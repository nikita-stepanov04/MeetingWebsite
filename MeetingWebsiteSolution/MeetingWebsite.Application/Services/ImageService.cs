using MeetingWebsite.Domain.Interfaces;
using MeetingWebsite.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace MeetingWebsite.Application.Services
{
    public class ImageService : IImageService
    {
        private IRepository<Image, long> _imageRepository;

        public ImageService(IRepository<Image, long> imageRepository)
        {
            _imageRepository = imageRepository;
        }

        public Task<Image> CreateAsync(Image image) =>
            _imageRepository.CreateAsync(image);

        public async Task<Image> CreateFromFormFileAsync(IFormFile formFile)
        {
            Image image = new();
            using (var stream = new MemoryStream())
            {
                await formFile.CopyToAsync(stream);
                image.Bitmap = stream.ToArray();
                image.MimeType = formFile.ContentType;
            }
            return await CreateAsync(image);
        }

        public ValueTask<Image?> FindByIdAsync(long id) =>
            _imageRepository.FindByIdAsync(id);
    }
}
