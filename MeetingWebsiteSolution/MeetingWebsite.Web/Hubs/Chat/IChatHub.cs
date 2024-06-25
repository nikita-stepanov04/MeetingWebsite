using MeetingWebsite.Domain.Models;

namespace MeetingWebsite.Web.Hubs.Chat
{
    public interface IChatHub
    {
        Task ReceiveMessageAsync(Message message);

        Task LoadChatAsync(List<Message> messages);

        Task SetMessageAsReadAsync(long messageId);
    }
}
