using MeetingWebsite.Domain.Models;

namespace MeetingWebsite.Domain.Interfaces
{
    public interface IChatService
    {
        Task<Chat?> GetChatAsync(long user1Id, long user2Id);

        Task<Chat?> GetChatAsync(Guid chatId);

        Task<Chat> CreateChatAsync(long user1Id, long user2Id);

        Task AddMessageToChatAsync(Message message, Guid chatId);

        Task SetMessageAsReadAsync(long messageId);

        Task<List<Message>?> GetMessagesFromChat(Guid chatId);

        Task<int> SaveChangesAsync();
    }
}
