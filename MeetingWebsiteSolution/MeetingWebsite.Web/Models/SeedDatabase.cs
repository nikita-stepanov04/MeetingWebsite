using MeetingWebsite.Domain.Interfaces;
using MeetingWebsite.Domain.Models;
using MeetingWebsite.Infrastracture.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace MeetingWebsite.Web.Models
{
    public static class SeedDatabase
    {
        public static void SeedIdentity(this WebApplication app) =>
            SeedIdentityAsync(app).Wait();

        public static void SeedData(this WebApplication app) =>
            SeedDataAsync(app).Wait();

        private static async Task SeedIdentityAsync(WebApplication app)
        {
            var serviceProvider = app.Services.CreateScope().ServiceProvider;
            var config = app.Configuration;

            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<AppRole>>();
            var userService = serviceProvider.GetRequiredService<IUserService>();

            string username = "user";
            string password = "password";
            string role = "user";

            if (await userManager.FindByNameAsync(username) == null)
            {
                if (await roleManager.FindByNameAsync(role) == null)
                {
                    await roleManager.CreateAsync(new AppRole(role));
                }

                User userdata = new User()
                {
                    Firstname = "Admin",
                    Secondname = "Admin",
                    Gender = UserGender.Male,
                    Birthday = DateOnly.ParseExact("14.05.2003", "dd.MM.yyyy")
                };
                await userService.CreateAsync(userdata);
                await userService.SaveChangesAsync();

                AppUser user = new()
                {
                    UserName = username,
                    UserDataId = userdata.UserId
                };

                IdentityResult result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }

        private static async Task SeedDataAsync(WebApplication app)
        {
            var interestService = app.Services.CreateScope()
                .ServiceProvider.GetRequiredService<IInterestService>();


            if ((await interestService.GetAllAsync()).Count() == 0)
            {
                List<Interest> interests = new()
                {
                    new Interest() { InterestType = "Traveling" },
                    new Interest() { InterestType = "Culinary" },
                    new Interest() { InterestType = "Sports" },
                    new Interest() { InterestType = "Arts" },
                    new Interest() { InterestType = "Music" },
                    new Interest() { InterestType = "Literature" },
                    new Interest() { InterestType = "Technology" },
                    new Interest() { InterestType = "Gaming" },
                    new Interest() { InterestType = "Fitnes" },
                };

                interests.ForEach(async interest =>
                    await interestService.CreateAsync(interest));
                await interestService.SaveChangesAsync();
            }
        }
    }
}
