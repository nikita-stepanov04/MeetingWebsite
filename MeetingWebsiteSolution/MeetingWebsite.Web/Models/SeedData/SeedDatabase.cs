using MeetingWebsite.Domain.Interfaces;
using MeetingWebsite.Domain.Models;
using MeetingWebsite.Infrastracture.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace MeetingWebsite.Web.Models.SeedData
{
    public static class SeedDatabase
    {
        public static void SeedData(this WebApplication app) =>
            SeedDataAsync(app).Wait();

        public static async Task SeedDataAsync(WebApplication app)
        {
            string userPassword = "password";
            string filePath = "Models/SeedData";
            string jsonFilePath = $"{filePath}/json";

            var serviceProvider = app.Services.CreateScope().ServiceProvider;
            var config = app.Configuration;

            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<AppRole>>();
            var userService = serviceProvider.GetRequiredService<IUserService>();
            var interestService = serviceProvider.GetRequiredService<IInterestService>();
            var imageService = serviceProvider.GetRequiredService<IImageService>();

            if ((await interestService.GetAllAsync()).Count() == 0)
            {
                string json = File.ReadAllText($"{jsonFilePath}/Interests.json");
                var interests = JsonConvert.DeserializeObject<List<Interest>>(json);
                interests?.ForEach(async interest => await interestService.CreateAsync(interest));
                await interestService.SaveChangesAsync();
            }

            if ((await userService.Count()) == 0)
            {
                Random rand = new(47);

                string json = File.ReadAllText($"{jsonFilePath}/Users.json");
                List<User> users = JsonConvert.DeserializeObject<List<User>>(json) ?? new();
                List<Interest> interests = await interestService.GetAllAsync();               

                foreach (var user in users)
                {
                    user.Image = await imageService
                        .CreateFromFileAsync($"{filePath}/{user.ImageLink!}");
                    user.Interests = interests.SelectRandom(4, rand).ToList();
                    await userService.CreateAsync(user);
                }
                await userService.SaveChangesAsync();

                string friendshipJson = File.ReadAllText($"{jsonFilePath}/Friendship.json");
                var friendship = JsonConvert.DeserializeObject<List<Friendship>>(friendshipJson);

                foreach (var user in users)
                {
                    var userFriendsIds = friendship!
                        .Where(fr => fr.UserId == user.UserId)
                        .Select(fr => fr.FriendId);                    
                    user.Friends = users.Where(u => userFriendsIds.Contains(u.UserId)).ToList();
                    await userService.UpdateAsync(user);
                }

                for (int i = 0; i < users.Count; i++)
                {
                    AppUser appUser = new()
                    {
                        UserName = $"user{i + 1}",
                        UserDataId = users[i].UserId
                    };
                    await userManager.CreateAsync(appUser, userPassword);
                }
                await userService.SaveChangesAsync();
            }
        }

        private static IEnumerable<T> SelectRandom<T>(
            this IEnumerable<T> enumerable, int count, Random rand)
        {
            int enumerableCount = enumerable.Count();
            count = enumerableCount < count ? enumerableCount : count;
            return enumerable.OrderBy(i => rand.Next()).Take(count);
        }

        private class Friendship
        {
            public long UserId { get; set; }
            public long FriendId { get; set; }
        }
    }    
}
