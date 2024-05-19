using MeetingWebsite.Application.Services;
using MeetingWebsite.Domain.Interfaces;
using MeetingWebsite.Infrastracture.EFRepository;
using MeetingWebsite.Infrastracture.Models;
using MeetingWebsite.Web.Models;

namespace MeetingWebsite.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddIdentityServices(builder.Configuration);

            builder.Services.AddScoped(typeof(IRepository<,>), typeof(EFRepository<,>));
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IInterestService, InterestService>();
            builder.Services.AddScoped<IImageService, ImageService>();            

            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapDefaultControllerRoute();

            app.SeedIdentity();
            app.SeedData();

            app.Run();
        }
    }
}
