using MeetingWebsite.Infrastracture.Models.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.CompilerServices;
using System.Text;

namespace MeetingWebsite.Web.Models
{
    public static class IdentityServices
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services,
            IConfiguration config)
        {
            string? jwtSecret = config["JwtSecret"];
            if (jwtSecret != null)
            {
                services.AddIdentity<AppUser, AppRole>(opts =>
                {
                    opts.Password.RequiredLength = 6;
                    opts.Password.RequireNonAlphanumeric = false;
                    opts.Password.RequireLowercase = false;
                    opts.Password.RequireUppercase = false;
                    opts.Password.RequireDigit = false;
                }).AddEntityFrameworkStores<IdentityContext>();

                services.AddAuthentication(opts =>
                {
                    opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    opts.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(opts =>
                {
                    opts.RequireHttpsMetadata = false; // Set to true when configure https
                    opts.SaveToken = true;
                    opts.TokenValidationParameters = new()
                    {
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSecret))
                    };
                    opts.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            context.Token = context.Request.Cookies["Bearer"];
                            return Task.CompletedTask;
                        }
                    };
                });
            }
            else
            {
                throw new Exception("JwtSecret is not defined");
            }

            return services;
        }
    }
}
