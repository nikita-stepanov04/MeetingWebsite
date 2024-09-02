namespace MeetingWebsite.Domain.Models
{
    public class PagingInfo
    {
        private int currentPage;
        public int CurrentPage 
        {
            get => currentPage;
            set => currentPage = value < 1 ? 1 : value;            
        }
        public int TotalPages { get; set; }
        public int ItemsPerPage { get; set; } = 6;
    }
}
