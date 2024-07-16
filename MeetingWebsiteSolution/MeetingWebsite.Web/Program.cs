using MeetingWebsite.Application.Services;
using MeetingWebsite.Domain.Interfaces;
using MeetingWebsite.Infrastracture.EFRepository;
using MeetingWebsite.Infrastracture.Models;
using MeetingWebsite.Web.Hubs.Chat;
using MeetingWebsite.Web.Hubs.Friendship;
using MeetingWebsite.Web.Models;
using MeetingWebsite.Web.Models.SeedData;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MeetingWebsite.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
           
            builder.Services.AddControllersWithViews()
                .AddNewtonsoftJson(opts =>
                {
                    opts.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    opts.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    opts.SerializerSettings.Converters.Add(new StringEnumConverter());
                });
            builder.Services.AddRazorPages();
            builder.Services.AddSignalR()
                .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddIdentityServices(builder.Configuration);

            builder.Services.AddScoped(typeof(IRepository<,>), typeof(EFRepository<,>));
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IInterestService, InterestService>();
            builder.Services.AddScoped<IImageService, ImageService>();            
            builder.Services.AddScoped<IFriendshipService, FriendshipService>();
            builder.Services.AddScoped<IChatService, ChatService>();
            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapDefaultControllerRoute();

            app.MapHub<ChatHub>("/wsChat").RequireAuthorization();
            app.MapHub<FriendshipHub>("/friendships").RequireAuthorization();

            app.SeedData();

            app.Run();
        }
    }
}
