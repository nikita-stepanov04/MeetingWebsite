using MeetingWebsite.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MeetingWebsite.Web.Filters
{
    public class ReturnPathFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;
            if (request.Query.ContainsKey("returnExecuted"))
            {
                var tempData = (context.Controller as Controller).TempData;
                Stack<string> stack = ReturnPathHelper.GetStack(tempData);
                stack.TryPop(out _);
                ReturnPathHelper.SetStack(tempData, stack);
            }
        }
    }
}
