using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;

namespace MeetingWebsite.Web.Helpers
{
    public static class ReturnPathHelper
    {
        public static void PushNewPage(ITempDataDictionary tempData, HttpContext context)
        {
            Stack<string> returnPathStack = GetStack(tempData);
            string returnPath = CurrentPath(context);
            if (returnPathStack.Contains(returnPath))
                while (returnPath != returnPathStack.Pop()) { }
            returnPathStack.Push(returnPath);            
            SetStack(tempData, returnPathStack);
        }

        public static string GetReturnPath(ITempDataDictionary tempData, HttpContext context)
        {
            Stack<string> returnPathStack = GetStack(tempData);
            string lastPath = returnPathStack.Pop();
            string? returnPath;
            if (CurrentPath(context) == lastPath)
            {
                returnPathStack.TryPeek(out returnPath);
                returnPathStack.Push(lastPath);
            }
            else
                returnPath = lastPath;
            SetStack(tempData, returnPathStack);
            return returnPath ?? "";
        }

        private static string _returnPathStackKey = "returnPathStack";

        private static string CurrentPath(HttpContext context) => 
            context.Request.Path.ToString() + context.Request.QueryString;

        public static Stack<string> GetStack(ITempDataDictionary tempData) =>
            JsonConvert.DeserializeObject<Stack<string>>(
                tempData.Peek(_returnPathStackKey) as string ?? "") ?? new();

        public static void SetStack(ITempDataDictionary tempData, Stack<string> stack) => 
            tempData[_returnPathStackKey] = JsonConvert.SerializeObject(stack.Reverse());
    }
}
