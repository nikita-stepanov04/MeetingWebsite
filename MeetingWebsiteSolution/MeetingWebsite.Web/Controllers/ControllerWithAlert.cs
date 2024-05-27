using Microsoft.AspNetCore.Mvc;

namespace MeetingWebsite.Web.Controllers
{
    public class ControllerWithAlert : Controller
    {
        public void AlertDanger(string message) => AlertBase("danger", message);
        public void AlertSuccess(string message) => AlertBase("success", message);
        public void AlertWarning(string message) => AlertBase("warning", message);

        public void ConditionAlert(bool condition, string ifConditionMessage, string elseMessage)
        {
            if (condition)
                AlertSuccess(ifConditionMessage);
            else
                AlertDanger(elseMessage);
        }

        private void AlertBase(string alertType, string message)
        {
            TempData["AlertMessage"] = message;
            TempData["AlertType"] = alertType;
        }
    }
}
