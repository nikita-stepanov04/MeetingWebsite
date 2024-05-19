using MeetingWebsite.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MeetingWebsite.Web.Components
{
    public class InterestsListViewComponent : ViewComponent
    {
        private IInterestService _interestService;

        public InterestsListViewComponent(IInterestService interestService)
        {
            _interestService = interestService;
        }

        public async Task<IViewComponentResult> InvokeAsync(IEnumerable<int> checkedInterestsIds) =>
            View(await _interestService.GetAllAsync());
    }
}
