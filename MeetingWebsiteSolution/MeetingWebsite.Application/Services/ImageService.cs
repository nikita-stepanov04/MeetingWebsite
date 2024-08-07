﻿using MeetingWebsite.Application.Interfaces;
using MeetingWebsite.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using DomainImage = MeetingWebsite.Domain.Models.Image;

namespace MeetingWebsite.Application.Services
{
#pragma warning disable CA1416
    public class ImageService : IImageService
    {
        private readonly int _defaultWidth = 500;
        private readonly int _defaultHeight = 500;

        private readonly int _maxCompressWidth = 800;
        private readonly int _maxCompressHeight = 800;

        private readonly string _mimeType = "image/jpeg";
        private readonly ImageFormat _imageType = ImageFormat.Jpeg;
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
            return await CreateAsync(await
                GetImageFromFormFileAsync(formFile, CompressImageInStream));
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
            using (Bitmap original = new(stream))
            {
                Resolution crop = GetCroppedImageResolution(original);

                // Span to center crop area on the original image
                int cropXSpan = (original.Width - crop.Width) / 2;
                int cropYSpan = (original.Height - crop.Height) / 2;

                Rectangle cropArea = new(cropXSpan, cropYSpan, crop.Width, crop.Height);
                using (Bitmap cropImage = new(cropArea.Width, cropArea.Height))
                {
                    using (Graphics graphics = Graphics.FromImage(cropImage))
                    {
                        graphics.DrawImage(original,
                            new Rectangle(0, 0, cropImage.Width, cropImage.Height),
                            cropArea, GraphicsUnit.Pixel);
                    }
                    stream.SetLength(0);
                    stream.Seek(0, SeekOrigin.Begin);
                    cropImage.Save(stream, _imageType);
                }
            }
        }

        private void CompressImageInStream(MemoryStream stream)
        {
            using (Bitmap original = new(stream))
            {
                Resolution compress = GetCompressedImageResolution(original);

                using (Bitmap compressedImage = new Bitmap(compress.Width, compress.Height))
                {
                    using (Graphics graphics = Graphics.FromImage(compressedImage))
                    {
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = SmoothingMode.HighQuality;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        graphics.CompositingQuality = CompositingQuality.HighQuality;

                        graphics.DrawImage(original, 0, 0, compress.Width, compress.Height);
                    }
                    stream.SetLength(0);
                    stream.Seek(0, SeekOrigin.Begin);
                    compressedImage.Save(stream, _imageType);
                }
            }
        }

        private Resolution GetCompressedImageResolution(Bitmap original)
        {
            Resolution res = new();
            if (original.Width > _maxCompressWidth || original.Height > _maxCompressHeight)
            {
                if (original.Width > original.Height)
                {
                    res.Width = _maxCompressWidth;
                    res.Height = (int)((float)original.Height / original.Width * _maxCompressWidth);
                }
                else
                {
                    res.Height = _maxCompressHeight;
                    res.Width = (int)((float)original.Width / original.Height * _maxCompressHeight);
                }
            }
            else
            {
                res.Width = original.Width;
                res.Height = original.Height;
            }
            return res;
        }

        private Resolution GetCroppedImageResolution(Bitmap original)
        {
            int widthDiff = original.Width - _defaultWidth;
            int heightDiff = original.Height - _defaultHeight;

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
            return res;
        }

        private async Task<DomainImage> GetImageFromFormFileAsync(
            IFormFile formFile, Action<MemoryStream> action)
        {
            DomainImage image = new();
            using (MemoryStream stream = new())
            {
                await formFile.CopyToAsync(stream);
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
            using (MemoryStream stream = new(File.ReadAllBytes(filePath)))
            {
                CropImageInStream(stream);
                image.Bitmap = stream.ToArray();
                image.MimeType = _mimeType;
            }
            return image;
        }
#pragma warning restore

        struct Resolution
        {
            public int Width { get; set; }
            public int Height { get; set; }
        }
    }
}
