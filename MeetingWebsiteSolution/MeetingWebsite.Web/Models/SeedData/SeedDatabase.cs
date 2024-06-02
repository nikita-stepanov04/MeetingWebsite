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

            var serviceProvider = app.Services.CreateScope().ServiceProvider;
            var config = app.Configuration;

            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<AppRole>>();
            var userService = serviceProvider.GetRequiredService<IUserService>();
            var interestService = serviceProvider.GetRequiredService<IInterestService>();
            var imageService = serviceProvider.GetRequiredService<IImageService>();

            if ((await interestService.GetAllAsync()).Count() == 0)
            {
                string json = File.ReadAllText("Models/SeedData/json/Interests.json");
                var interests = JsonConvert.DeserializeObject<List<Interest>>(json);
                interests?.ForEach(async interest => await interestService.CreateAsync(interest));
                await interestService.SaveChangesAsync();
            }

            if ((await userService.Count()) == 0)
            {
                string json = File.ReadAllText("Models/SeedData/json/Users.json");
                List<User> users = JsonConvert.DeserializeObject<List<User>>(json) ?? new();
                List<Interest> interests = await interestService.GetAllAsync();

                Random rand = new(47);
                foreach (var user in users)
                {
                    user.Image = await imageService
                        .CreateFromFileAsync($"Models/SeedData/{user.ImageLink!}");
                    user.Interests = interests.SelectRandom(4, rand).ToList();
                    await userService.CreateAsync(user);                    
                }
                await userService.SaveChangesAsync();

                int counter = 1;
                foreach (var user in users)
                {
                    AppUser appUser = new()
                    {
                        UserName = $"user{counter}",
                        UserDataId = user.UserId
                    };
                    await userManager.CreateAsync(appUser, userPassword);

                    user.Friends = users.SelectRandom(4, rand).ToList();
                    await userService.UpdateAsync(user);
                    counter++;
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
    }    
}
