using MeetingWebsite.Domain.Models;

namespace MeetingWebsite.Web.Models.ViewModels
{
    public class MeetingViewModel
    {
        public IEnumerable<User> Users { get; set; } = Enumerable.Empty<User>();
        public PagingInfo PagInfo { get; set; } = new();
        public FilterInfo? FilterInfo { get; set; }
    }
}
