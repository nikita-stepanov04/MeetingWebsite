using MeetingWebsite.Domain.Interfaces;
using MeetingWebsite.Domain.Models;
using MeetingWebsite.Infrastracture.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace MeetingWebsite.Web.Hubs.Friendship
{
    [Authorize]
    public class FriendshipHub : Hub<IFriendshipHub>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserService _userService;
        private readonly IFriendshipService _friendshipService;

        public FriendshipHub(UserManager<AppUser> userManager,
            IUserService userService,
            IFriendshipService friendshipService)
        {
            _userManager = userManager;
            _userService = userService;
            _friendshipService = friendshipService;
        }

        public string UserIdStr => (Context.Items["userId"] as string)!;
        public long UserId => Convert.ToInt64(UserIdStr);

        public async override Task OnConnectedAsync()
        {
            AppUser appUser = (await _userManager.FindByNameAsync(Context.User?.Identity?.Name!))!;
            string userId = appUser.UserDataId.ToString();

            Context.Items["userId"] = userId;
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, UserIdStr);
            await base.OnDisconnectedAsync(exception);
        }        

        public async Task SendFriendshipRequestAsync(long receiverId)
        {
            bool result = await _friendshipService.SendFriendshipRequestAsync(UserId, receiverId);
            await GenerateResponse(result, receiverId, "Faild to send friendship request");
        }

        public async Task DeleteFromFriendsAsync(long friendId)
        {
            bool result = await _friendshipService.DeleteFromFriendsAsync(UserId, friendId);
            await GenerateResponse(result, friendId, "Faild to delete from friends");
        }

        public async Task AcceptFriendshipRequestAsync(long senderId)
        {
            bool result = await _friendshipService.AcceptFriendshipRequestAsync(senderId, UserId);
            await GenerateResponse(result, senderId, "Faild to accept friendship request");
        }

        public async Task RejectFriendshipRequestAsync(long senderId)
        {
            bool result = await _friendshipService.RejectFriendshipRequestAsync(senderId, UserId);
            await GenerateResponse(result, senderId, "Faild to reject friendship request");
        }

        public async Task CancelFriendshipRequestAsync(long receiverId)
        {
            bool result = await _friendshipService.RejectFriendshipRequestAsync(UserId, receiverId);
            await GenerateResponse(result, receiverId, "Request has been already accepted or rejected");
        }

        private async Task UpdateFriendshipRequestsCountAsync(string group)
        {
            User user = (await _userService.FindByIdAsync(Convert.ToInt64(group)))!;
            int count = await _friendshipService.GetFriendshipRequestsCountAsync(user);
            await Clients.Group(group).UpdateFriendshipRequestsCountAsync(count);
        }

        public async Task UpdateFriendshipStatusRequestAsync(long[] ids) => 
            await UpdateFriendshipStatusRequestByGroupAsync(UserIdStr, ids);

        private async Task UpdateFriendshipStatusRequestByGroupAsync(
            string group, params long[] ids)
        {
            var statusList = await _friendshipService
                .GetFriendshipStatus(Convert.ToInt64(group), ids);
            await Clients.Group(group).UpdateFriendshipStatusResponseAsync(statusList);
        }

        private async Task GenerateResponse(bool result, long id, string error)
        {
            string userIdStr = UserIdStr;
            string friendIdStr = id.ToString();
            long userId = Convert.ToInt64(userIdStr);

            if (result)
            {                
                await UpdateFriendshipRequestsCountAsync(userIdStr);
                await UpdateFriendshipRequestsCountAsync(friendIdStr);
                await UpdateFriendshipStatusRequestByGroupAsync(userIdStr, id);
                await UpdateFriendshipStatusRequestByGroupAsync(friendIdStr, userId);
            }
            else
                await Clients.Group(userIdStr).ShowErrorAsync(error);
        }
    }
}
