using MeetingWebsite.Domain.Interfaces;
using MeetingWebsite.Infrastracture.Models.Identity;
using MeetingWebsite.Web.Controllers;
using MeetingWebsite.Web.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace MeetingWebsite.Tests
{
    public class AccountControllerTests
    {
        private Mock<UserManager<AppUser>> _userManagerMock;
        private Mock<SignInManager<AppUser>> _signInManagerMock;
        private Mock<IUserService> _userDataServiceMock;
        private Mock<IInterestService> _interestServiceMock;
        private Mock<IImageService> _imageServiceMock;
        private Mock<IFriendshipService> _friendhsipServiceMock;
        private Mock<IConfiguration> _configMock;
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;

        [Fact]
        public async Task LoginReturnsTokenAsCookie()
        {
            var controller = CreateController();
            LoginViewModel model = new()
            {
                Username = "testuser",
                Password = "password123$"
            };
            string jwtSecret = "QqmoIgOYb7peYUbpjuM7IpbO688xpqXc";

            _userManagerMock.Setup(um => um.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new AppUser { UserName = "testuser" });
            _signInManagerMock.Setup(sm => sm.CheckPasswordSignInAsync(
                It.IsAny<AppUser>(), It.IsAny<string>(), false))
                    .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
            _configMock.Setup(c => c["JwtSecret"]).Returns(jwtSecret);

            var result = await controller.LoginPost(model);
            var context = controller.HttpContext;

            Assert.NotNull(context.Response?.Headers
                .FirstOrDefault(h => h.ToString().StartsWith("Bearer=")));
        }

        private AccountController CreateController()
        {
            _userManagerMock = new Mock<UserManager<AppUser>>(
                new Mock<IUserStore<AppUser>>().Object, null, null, null, null, null, null, null, null);

            _signInManagerMock = new Mock<SignInManager<AppUser>>(
                _userManagerMock.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<AppUser>>().Object,
                null, null, null, null);

            _userDataServiceMock = new Mock<IUserService>();
            _interestServiceMock = new Mock<IInterestService>();
            _imageServiceMock = new Mock<IImageService>();
            _friendhsipServiceMock = new Mock<IFriendshipService>();
            _configMock = new Mock<IConfiguration>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            var context = new DefaultHttpContext();
            _httpContextAccessorMock.Setup(_ => _.HttpContext).Returns(context);

            return new AccountController(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _configMock.Object,
                _userDataServiceMock.Object,
                _interestServiceMock.Object,
                _imageServiceMock.Object,
                _friendhsipServiceMock.Object
            )
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = context
                }
            };
        }
    }
}
