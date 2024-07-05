namespace MeetingWebsite.Domain.Models
{
    public class Chat
    {
        public Guid ChatId { get; set; }
        public string ChatName { get; set; } = string.Empty;

        public long User1Id { get; set; }
        public long User2Id { get; set; }

        public User? User1 { get; set; }
        public User? User2 { get; set; }

        public List<Message> Messages { get; set; } = [];
        public List<Image> Images { get; set; } = [];
    }
}
