using System.ComponentModel.DataAnnotations;

namespace MeetingWebsite.Domain.Models
{
    public class FilterInfo
    {
        public int AgeMin { get; set; }
        public int AgeMax { get; set; }
        public List<UserGender> Genders { get; set; } = new();
        public List<long> CheckInterestsIds { get; set; } = new();
    }
}
