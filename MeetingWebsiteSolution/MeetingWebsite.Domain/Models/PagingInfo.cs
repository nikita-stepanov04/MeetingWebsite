namespace MeetingWebsite.Domain.Models
{
    public class PagingInfo
    {
        private int currentPage;
        public int CurrentPage 
        {
            get => currentPage;
            set
            {
                if (value < 1)
                {
                    value = 1;
                }
                currentPage = value;
            }
        }
        public int TotalPages { get; set; }
        public int ItemsPerPage { get; set; } = 6;
    }
}
