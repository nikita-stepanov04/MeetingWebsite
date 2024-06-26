﻿using MeetingWebsite.Domain.Interfaces;
using MeetingWebsite.Domain.Models;
using MeetingWebsite.Infrastracture.Models.Identity;
using MeetingWebsite.Web.Helpers;
using MeetingWebsite.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MeetingWebsite.Web.Controllers
{
    [Authorize]
    [Route("/")]
    [AutoValidateAntiforgeryToken]
    public class MeetingController : MeetingWebsiteViewController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserService _userService;
        private readonly IFriendshipService _friendshipService;

        public MeetingController(UserManager<AppUser> userManager,
            IUserService userService,
            IFriendshipService friendshipService)
        {
            _userManager = userManager;
            _userService = userService;
            _friendshipService = friendshipService;
        }

        public Task<AppUser> AppUserTask => _userManager.FindByNameAsync(User.Identity?.Name!)!;

        [HttpGet()]
        [HttpGet("page/{page:int?}")]
        public async Task<IActionResult> Index(MeetingViewModel? model, int? page)
        {
            model = model ?? new();
            model.PagInfo.CurrentPage = page ?? model.PagInfo.CurrentPage;

            User? user = await _userService.FindByIdAsync((await AppUserTask).UserDataId);
            if (user != null)
            {
                model.Users = await _userService
                    .FindUsersByFiltersAndPagingInfo(model.FilterInfo, model.PagInfo, [user]);
                model.FriendshipRequestSendersIds = (await _friendshipService
                    .GetFriendshipRequestsAsync(user)).Select(fr => fr.SenderId).ToHashSet();
                model.SentFriendshipRequestsReceiversIds = (await _friendshipService
                    .GetSentRequestsAsync(user)).Select(fr => fr.ReceiverId).ToHashSet();
                model.FriendsIds = (user.Friends ?? Enumerable.Empty<User>())
                    .Select(u => u.UserId).ToHashSet();
            }
            return View(model);
        }

        [HttpGet("user/{id:long}")]
        public async Task<IActionResult> GetUser(long id)
        {
            var user = await _userService.FindByIdAsync((await AppUserTask).UserDataId);
            var friend = await _userService.FindByIdAsync(id);             
            if (user != null && friend != null)
            {
                return View("User", new UserViewModel()
                {
                    User = friend,
                    FriendhipInfo = await _friendshipService.GetFriendhipInfo(user, friend)
                });
            }
            return NotFound();
        }

        [HttpPost("send-friendship-request")]
        public async Task<IActionResult> SendFriendshipRequest(long receiverId)
        {
            bool result = await _friendshipService.SendFriendshipRequestAsync(
                (await AppUserTask).UserDataId, receiverId);
            ConditionAlert(result,
                "Frienship request has been sent", "Faild to send friendship request");
            return Redirect(ReturnPathHelper.GetReturnPath(TempData, HttpContext));
        }

        [HttpPost("accept-friendship-request")]
        public async Task<IActionResult> AcceptFriendshipRequest(long senderId)
        {
            bool result = await _friendshipService.AcceptFriendshipRequestAsync(
                senderId, (await AppUserTask).UserDataId);
            ConditionAlert(result,
                "Frienship request has been accepted", "Faild to accept friendship request");
            return Redirect(ReturnPathHelper.GetReturnPath(TempData, HttpContext));
        }

        [HttpPost("reject-friendship-request")]
        public async Task<IActionResult> RejectFriendshipRequest(long senderId)
        {
            bool result = await _friendshipService.RejectFriendshipRequestAsync(
                 senderId, (await AppUserTask).UserDataId);
            ConditionAlert(result,
                "Frienship request has been rejected", "Faild to reject friendship request");
            return Redirect(ReturnPathHelper.GetReturnPath(TempData, HttpContext));
        }

        [HttpPost("cancel-friendship-request")]
        public async Task<IActionResult> CancelFriendshipRequest(long receiverId)
        {
            bool result = await _friendshipService.RejectFriendshipRequestAsync(
                (await AppUserTask).UserDataId, receiverId);
            ConditionAlert(result,
                "Frienship request has been canceled", "Request has been already accepted or rejected");
            return Redirect(ReturnPathHelper.GetReturnPath(TempData, HttpContext));
        }

        [HttpPost("delete-from-friends")]
        public async Task<IActionResult> DeleteFromFriends(long friendId)
        {
            bool result = await _friendshipService.DeleteFromFriendsAsync(
                (await AppUserTask).UserDataId, friendId);
            ConditionAlert(result,
                "Deletion from friends succeded", "Faild to delete from friends");
            return Redirect(ReturnPathHelper.GetReturnPath(TempData, HttpContext));
        }
    }
}
