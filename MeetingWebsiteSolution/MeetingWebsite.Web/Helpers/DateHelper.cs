namespace MeetingWebsite.Web.Helpers
{
    public static class DateHelper
    {
        public static int GetFullYears(this DateOnly date)
        {
            DateTime dateTime = date.ToDateTime(TimeOnly.MinValue);
            int yearsDifference = DateTime.Today.Year - date.Year;
            if (DateTime.Today < dateTime.AddYears(yearsDifference))
            {
                yearsDifference--;
            }
            return yearsDifference;
        }            
    }
}
