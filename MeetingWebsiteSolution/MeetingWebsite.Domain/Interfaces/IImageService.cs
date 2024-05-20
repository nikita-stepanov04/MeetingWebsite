﻿using MeetingWebsite.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace MeetingWebsite.Domain.Interfaces
{
    public interface IImageService
    {
        ValueTask<Image?> FindByIdAsync(long id);

        Task<Image> CreateAsync(Image image);

        Task<Image> CreateFromFormFileAsync(IFormFile formFile);
        
    }
}