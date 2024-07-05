using MeetingWebsite.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace MeetingWebsite.Domain.Interfaces
{
    public interface IImageService
    {
        ValueTask<Image?> FindByIdAsync(long id);

        Task<Image> CreateAsync(Image image);

        Task<Image> CreateFromFileAsync(string filePath);        

        Task<Image> CreateFromFormFileAsync(IFormFile formFile);

        Task<Image> CreateFromFormFileCompressedOriginalAspectRatio(IFormFile formFile);

        Task<Image> UpdateFromFormFileAsync(Image image, IFormFile formFile);

        Task<Image?> GetImageInfoAsync(long imageId);

        ValueTask<Image> Remove(Image image);

        public Task<int> SaveChangesAsync();
    }
}
