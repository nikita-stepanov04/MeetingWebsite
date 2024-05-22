using MeetingWebsite.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace MeetingWebsite.Web.Helpers
{
    public static class ImageLinkHelper
    {
        public static string? GetImageUrl(this IUrlHelper helper, User? user)
        {
            return user?.ImageId == null
                ? null
                : helper.Action("GetImage", "Image", new { id = user.ImageId });
        }
    }
}
