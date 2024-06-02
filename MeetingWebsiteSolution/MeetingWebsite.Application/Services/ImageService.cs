using MeetingWebsite.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Drawing;
using DomainImage = MeetingWebsite.Domain.Models.Image;

namespace MeetingWebsite.Application.Services
{
    public class ImageService : IImageService
    {
        private readonly int defaultWidth = 500;
        private readonly int defaultHeight = 500;

        private readonly IRepository<DomainImage, long> _imageRepository;

        public ImageService(IRepository<DomainImage, long> imageRepository)
        {
            _imageRepository = imageRepository;
        }

        public Task<DomainImage> CreateAsync(DomainImage image) =>
            _imageRepository.CreateAsync(image);

        public async Task<DomainImage> CreateFromFormFileAsync(IFormFile formFile)
        {
            DomainImage image = new();
            using (MemoryStream stream = new())
            { 
                await formFile.CopyToAsync(stream);
                CropImageInStream(stream);

                image.Bitmap = stream.ToArray();
                image.MimeType = formFile.ContentType;
            }
            return await CreateAsync(image);
        }

        public async Task<DomainImage> CreateFromFileAsync(string filePath)
        {
            DomainImage image = new();

            if (!File.Exists(filePath))
                    throw new FileNotFoundException($"File with path: {filePath} was not found");
            using (MemoryStream stream = new(File.ReadAllBytes(filePath)))
            {                
                CropImageInStream(stream);

                image.Bitmap = stream.ToArray();
                image.MimeType = "image/jpeg";
            }
            return await CreateAsync(image);
        }

        public ValueTask<DomainImage?> FindByIdAsync(long id) =>
            _imageRepository.FindByIdAsync(id);

#pragma warning disable CA1416
        private void CropImageInStream(MemoryStream stream)
        {      
            using (Bitmap original = new(stream))
            {
                Resolution crop = GetCroppedDomainImageResolution(original);

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
                    cropImage.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
            }
        }

        private Resolution GetCroppedDomainImageResolution(Bitmap original)
        {
            int widthDiff = original.Width - defaultWidth;
            int heightDiff = original.Height - defaultHeight;

            Resolution res = new();

            if (widthDiff < 0 || heightDiff < 0)
            {
                if (widthDiff < heightDiff)
                {
                    res.Width = defaultWidth + widthDiff;
                    res.Height = res.Width * defaultHeight / defaultWidth;
                }
                else if (heightDiff < widthDiff)
                {
                    res.Height = defaultHeight + heightDiff;
                    res.Width = res.Height * defaultWidth / defaultHeight;
                }
                else
                {
                    res.Width = defaultWidth + widthDiff;
                    res.Height = defaultHeight + heightDiff;
                }
            }
            else
            {
                res.Width = defaultWidth;
                res.Height = defaultHeight;
            }
            return res;
        }
#pragma warning restore

        struct Resolution
        {
            public int Width { get; set; }
            public int Height { get; set; }
        }
    }
}
