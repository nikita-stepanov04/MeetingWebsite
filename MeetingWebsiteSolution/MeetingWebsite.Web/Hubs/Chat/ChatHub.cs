using MeetingWebsite.Domain.Interfaces;
using DomainChat = MeetingWebsite.Domain.Models.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MeetingWebsite.Domain.Models;
using MeetingWebsite.Infrastracture.Models.Identity;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;

namespace MeetingWebsite.Web.Hubs.Chat
{
    [Authorize]
    public class ChatHub : Hub<IChatHub>
    {
        private readonly IChatService _chatService;
        private readonly IUserService _userService;
        private readonly UserManager<AppUser> _userManager;

        public ChatHub(IChatService chatService,
            IUserService userService,
            UserManager<AppUser> userManager)
        {
            _chatService = chatService;
            _userService = userService;
            _userManager = userManager;
        }

        public string ChatId => Context.Items["chatId"]!.ToString()!;
        public long UserId => Convert.ToInt64(Context.Items["userId"]!);        

        public override async Task OnConnectedAsync()
        {
            string? chatId = Context.GetHttpContext()?.Request.Query["chatId"];
            if (chatId == null)
            {
                throw new HubException("Chat id is not specified");
            }

            DomainChat? chat = await _chatService.GetChatAsync(Guid.Parse(chatId));
            if (chat != null)
            {
                AppUser appUser = (await _userManager
                    .FindByNameAsync(Context.User?.Identity?.Name!))!;
                if (chat.User1Id == appUser.UserDataId || chat.User2Id == appUser.UserDataId)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
                    Context.Items["chatId"] = chatId;
                    Context.Items["userId"] = appUser.UserDataId;
                }
                else
                    throw new HubException("User is not a chat owner");                
            }
            else
                throw new HubException("Chat with specified id doesn't exist");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, ChatId);            
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessageAsync(string text)
        {
            long userId = UserId;
            string chatId = ChatId;

            Message message = new()
            {
                Text = text,
                AuthorId = userId
            };
            await _chatService.AddMessageToChatAsync(message, Guid.Parse(chatId));
            await Clients.Group(chatId).ReceiveMessageAsync(message);
        }

        public async Task LoadChatAsync()
        {
            string chatId = ChatId;
            List<Message> messages = (await _chatService
                .GetMessagesFromChat(Guid.Parse(chatId)) ?? new());
            await Clients.Caller.LoadChatAsync(messages);
        }

        public async Task SetMessageAsReadAsync(long messageId)
        {
            await _chatService.SetMessageAsReadAsync(messageId);
            await Clients.Group(ChatId).SetMessageAsReadAsync(messageId);
        }
    }
}
