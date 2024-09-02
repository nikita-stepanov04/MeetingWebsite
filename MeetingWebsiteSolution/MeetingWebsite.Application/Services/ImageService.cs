using MeetingWebsite.Application.Interfaces;
using MeetingWebsite.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using DomainImage = MeetingWebsite.Domain.Models.Image;

namespace MeetingWebsite.Application.Services
{
    public class ImageService : IImageService
    {
        private readonly int _defaultWidth = 500;
        private readonly int _defaultHeight = 500;

        private readonly int _maxCompressWidth = 800;
        private readonly int _maxCompressHeight = 800;

        private readonly string _mimeType = "image/jpeg";
        private readonly string ImageCachePrefix = "Image";

        private readonly IRepository<DomainImage, long> _imageRepository;
        private readonly IDistributedMeetingCache _cache;

        public ImageService(IRepository<DomainImage, long> imageRepository,
            IDistributedMeetingCache cache)
        {
            _imageRepository = imageRepository;
            _cache = cache;
        }

        public Task<DomainImage> CreateAsync(DomainImage image) => 
            _imageRepository.CreateAsync(image);

        public async Task<DomainImage> CreateFromFormFileAsync(IFormFile formFile)
        {
            return await CreateAsync(await GetImageFromFormFileAsync(formFile, CropImageInStream));
        }

        public async Task<DomainImage> CreateFromFormFileCompressedOriginalAspectRatio(
            IFormFile formFile)
        {
            return await CreateAsync(await GetImageFromFormFileAsync(formFile, CompressImageInStream));
        }

        public async Task<DomainImage> CreateFromFileAsync(string filePath)
        {
            return await CreateAsync(GetImageFromFile(filePath));
        }

        public async Task<DomainImage> UpdateFromFormFileAsync(DomainImage image, IFormFile formFile)
        {
            DomainImage newImage = await
                GetImageFromFormFileAsync(formFile, CropImageInStream);
            image.MimeType = newImage.MimeType;
            image.Bitmap = newImage.Bitmap;
            await _imageRepository.UpdateAsync(image);
            await _cache.RemoveRecordAsync(ImageCachePrefix, image.ImageId);
            return newImage;
        }        

        public async ValueTask<DomainImage?> FindByIdAsync(long id)
        {
            DomainImage? image = await _cache.GetImageAsync(ImageCachePrefix, id);
            if (image == null)
            {
                image = await _imageRepository.FindByIdAsync(id);
                if (image != null)
                {
                    await _cache.SetRecordAsync(ImageCachePrefix, id, image);
                }
            }
            return image;
        }

        public async Task<DomainImage?> GetImageInfoAsync(long imageId)
        {
            DomainImage? image = await _cache.GetImageAsync(ImageCachePrefix, imageId);
            if (image == null)
            {
                image = await _imageRepository.GetQueryable()
                    .Where(i => i.ImageId == imageId)
                    .Select(i => new DomainImage()
                    {
                        ImageId = i.ImageId,
                        MimeType = i.MimeType,
                        ChatId = i.ChatId
                    })
                    .FirstOrDefaultAsync();                
                return image;
            }  
            image.Bitmap = [];
            return image;
        }

        public async ValueTask<DomainImage> Remove(DomainImage image)
        {
            await _cache.RemoveRecordAsync(ImageCachePrefix, image.ImageId);
            return await _imageRepository.DeleteAsync(image);
        }

        public Task<int> SaveChangesAsync() => _imageRepository.SaveChangesAsync();

        private void CropImageInStream(MemoryStream stream)
        {
            using (var image = Image.Load<Rgba32>(stream))
            {
                Resolution crop = GetCroppedImageResolution(image);

                image.Mutate(x => x.Crop(
                    new Rectangle(crop.XSpan, crop.YSpan, crop.Width, crop.Height)));

                stream.Seek(0, SeekOrigin.Begin);
                image.SaveAsJpeg(stream);
            }
        }

        private void CompressImageInStream(MemoryStream stream)
        {
            using (var image = Image.Load<Rgba32>(stream))
            {
                Resolution compress = GetCompressedImageResolution(image);

                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(compress.Width, compress.Height)
                }));

                stream.Seek(0, SeekOrigin.Begin);
                image.SaveAsJpeg(stream);
            }
        }

        private Resolution GetCompressedImageResolution(Image<Rgba32> image)
        {
            Resolution res = new();
            if (image.Width > _maxCompressWidth || image.Height > _maxCompressHeight)
            {
                if (image.Width > image.Height)
                {
                    res.Width = _maxCompressWidth;
                    res.Height = (int)((float)image.Height / image.Width * _maxCompressWidth);
                }
                else
                {
                    res.Height = _maxCompressHeight;
                    res.Width = (int)((float)image.Width / image.Height * _maxCompressHeight);
                }
            }
            else
            {
                res.Width = image.Width;
                res.Height = image.Height;
            }
            return res;
        }

        private Resolution GetCroppedImageResolution(Image<Rgba32> image)
        {
            int widthDiff = image.Width - _defaultWidth;
            int heightDiff = image.Height - _defaultHeight;

            Resolution res = new();

            if (widthDiff < 0 || heightDiff < 0)
            {
                if (widthDiff < heightDiff)
                {
                    res.Width = _defaultWidth + widthDiff;
                    res.Height = res.Width * _defaultHeight / _defaultWidth;
                }
                else if (heightDiff < widthDiff)
                {
                    res.Height = _defaultHeight + heightDiff;
                    res.Width = res.Height * _defaultWidth / _defaultHeight;
                }
                else
                {
                    res.Width = _defaultWidth + widthDiff;
                    res.Height = _defaultHeight + heightDiff;
                }
            }
            else
            {
                res.Width = _defaultWidth;
                res.Height = _defaultHeight;
            }

            res.XSpan = (image.Width - res.Width) / 2;
            res.YSpan = (image.Height - res.Height) / 2;

            return res;
        }

        private async Task<DomainImage> GetImageFromFormFileAsync(
            IFormFile formFile, Action<MemoryStream> action)
        {
            DomainImage image = new();
            using (MemoryStream stream = new())
            {
                await formFile.CopyToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);
                action(stream);

                image.Bitmap = stream.ToArray();                
                image.MimeType = formFile.ContentType;
            }
            return image;
        }

        private DomainImage GetImageFromFile(string filePath)
        {
            DomainImage image = new();

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File with path: {filePath} was not found");
            using (MemoryStream stream = new())
            {
                stream.Write(File.ReadAllBytes(filePath));
                stream.Seek(0, SeekOrigin.Begin);
                CropImageInStream(stream);
                image.Bitmap = stream.ToArray();
                image.MimeType = _mimeType;
            }
            return image;
        }

        struct Resolution
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public int XSpan { get; set; }
            public int YSpan { get; set; }
        }
    }
}