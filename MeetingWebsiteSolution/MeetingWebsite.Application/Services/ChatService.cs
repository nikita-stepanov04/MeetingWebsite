using MeetingWebsite.Domain.Interfaces;
using MeetingWebsite.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetingWebsite.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly IRepository<Chat, Guid> _chatRepository;
        private readonly IRepository<Message, long> _messageRepository;

        public ChatService(IRepository<Chat, Guid> chatRepository,
            IRepository<Message, long> messageRepository)
        {
            _chatRepository = chatRepository;
            _messageRepository = messageRepository;
        }

        public Task<Chat?> GetChatAsync(long user1Id, long user2Id)
        {
            string chatName = GetChatName(user1Id, user2Id);
            return _chatRepository.GetQueryable()
                .FirstOrDefaultAsync(c => c.ChatName == chatName);
        }

        public Task<Chat?> GetChatAsync(Guid chatId)
        {
            return _chatRepository.GetQueryable()
                .FirstOrDefaultAsync(c => c.ChatId == chatId);
        }

        public Task<List<Message>?> GetMessagesFromChat(Guid chatId)
        {
            return _chatRepository.GetQueryable()
                .Include(c => c.Messages)
                .Where(c => c.ChatId == chatId)
                .Select(c => c.Messages)
                .FirstOrDefaultAsync();
        }
 
        public Task<Chat> CreateChatAsync(long user1Id, long user2Id)
        {
            Chat chat = new()
            {
                User1Id = user1Id,
                User2Id = user2Id,
                ChatName = GetChatName(user1Id, user2Id)
            };
            return _chatRepository.CreateAsync(chat);
        }

        public async Task AddMessageToChatAsync(Message message, Guid chatId)
        {
            Chat? chat = await _chatRepository.FindByIdAsync(chatId);
            if (chat != null &&
                (chat.User1Id == message.AuthorId || chat.User2Id == message.AuthorId))
            {
                message.ChatId = chatId;
                message.CreatedAt = DateTime.UtcNow;
                await _messageRepository.CreateAsync(message);
                await _chatRepository.SaveChangesAsync();
            }
        }

        public async Task SetMessageAsReadAsync(long messageId)
        {
            Message? message = await _messageRepository.FindByIdAsync(messageId);
            if (message != null)
            {
                message.IsRead = true;
                await _chatRepository.SaveChangesAsync();
            }
        }

        public Task<int> SaveChangesAsync() =>
            _chatRepository.SaveChangesAsync();

        private string GetChatName(long user1Id, long user2Id) =>
            user1Id <= user2Id
                ? $"{user1Id}_{user2Id}"
                : $"{user2Id}_{user1Id}";
    }
}
