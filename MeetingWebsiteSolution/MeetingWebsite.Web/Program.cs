using MeetingWebsite.Infrastracture.Models;

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
            
            var app = builder.Build();

            app.Run();
        }
    }
}
