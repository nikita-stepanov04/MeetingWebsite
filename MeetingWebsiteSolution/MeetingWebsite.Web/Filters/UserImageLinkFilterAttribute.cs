using MeetingWebsite.Domain.Models;
using MeetingWebsite.Web.Helpers;
using MeetingWebsite.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MeetingWebsite.Web.Filters
{
    public class UserImageLinkFilterAttribute : ResultFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is ViewResult viewResult)
            {
                IUrlHelper url = (context.Controller as Controller)!.Url;
                _ = viewResult.Model switch
                {
                    User user => UserModelHandler(user, url),
                    UserViewModel model => UserViewModelHandler(model, url),
                    AccountViewModel model => AccountViewModelHandler(model, url),
                    MeetingViewModel model => MeetingViewModelHandler(model, url),
                    _ => default
                };
            }
        }

        private bool UserModelHandler(User user, IUrlHelper url)
        {
            user.ImageLink = url.GetImageUrl(user);
            return true;
        }

        private bool AccountViewModelHandler(AccountViewModel model, IUrlHelper url)
        {
            model.UserData.ImageLink = url.GetImageUrl(model.UserData);
            foreach (var user in model.Senders ?? [])
            {
                user.ImageLink = url.GetImageUrl(user);
            }
            return true;
        }

        private bool MeetingViewModelHandler(MeetingViewModel model, IUrlHelper url)
        {
            foreach (var user in model.Users ?? [])
            {
                user.ImageLink = url.GetImageUrl(user);
            }
            return true;
        }
        
        private bool UserViewModelHandler(UserViewModel model, IUrlHelper url)
        {
            model.User.ImageLink = url.GetImageUrl(model.User);
            return true;
        }
    }
}
