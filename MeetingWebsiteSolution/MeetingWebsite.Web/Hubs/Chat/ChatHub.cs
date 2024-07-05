using MeetingWebsite.Domain.Interfaces;
using MeetingWebsite.Domain.Models;
using MeetingWebsite.Infrastracture.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.SignalR;
using DomainChat = MeetingWebsite.Domain.Models.Chat;
using MeetingWebsite.Web.Helpers;

namespace MeetingWebsite.Web.Hubs.Chat
{
    [Authorize]
    public class ChatHub : Hub<IChatHub>
    {
        private readonly IChatService _chatService;
        private readonly IUserService _userService;
        private readonly IImageService _imageService;
        private readonly UserManager<AppUser> _userManager;

        public ChatHub(IChatService chatService,
            IUserService userService,
            UserManager<AppUser> userManager,
            IImageService imageService)
        {
            _chatService = chatService;
            _userService = userService;
            _userManager = userManager;
            _imageService = imageService;
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

        public async Task SendMessageAsync(Message message)
        {
            if (message.ImageId != null || message.Text != null)
            {
                long userId = UserId;
                string chatId = ChatId;

                message.AuthorId = userId;
                message.CreatedAt = DateTime.UtcNow;

                await _chatService.AddMessageToChatAsync(message, Guid.Parse(chatId));

                PopulateMessageWithImageLink(message);
                await Clients.Group(chatId).ReceiveMessageAsync(message);
            }
        }

        public async Task LoadChatAsync()
        {
            string chatId = ChatId;
            List<Message> messages = (await _chatService
                .GetMessagesFromChat(Guid.Parse(chatId)) ?? new());

            messages.ForEach(m => PopulateMessageWithImageLink(m));
            await Clients.Caller.LoadChatAsync(messages);
        }

        public async Task SetMessageAsReadAsync(long messageId)
        {
            await _chatService.SetMessageAsReadAsync(messageId);
            await Clients.Group(ChatId).SetMessageAsReadAsync(messageId);
        }        

        private void PopulateMessageWithImageLink(Message message)
        {
            if (message.ImageId != null)
            {
                message.ImageLink = ImageLinkHelper.GetImageUrl((long)message.ImageId);
            }
        }
    }
}
