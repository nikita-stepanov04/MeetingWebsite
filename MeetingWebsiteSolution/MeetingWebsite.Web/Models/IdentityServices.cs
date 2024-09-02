using MeetingWebsite.Infrastracture.Models;
using MeetingWebsite.Infrastracture.Models.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MeetingWebsite.Web.Models
{
    public static class IdentityServices
    {
        public static string GenerateToken(string username, string secret, DateTime expires)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: [new Claim(ClaimTypes.Name, username!)],
                expires: expires,
                signingCredentials: credentials
            );

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(token);
        }

        public static void SetTokenCookie(string token, HttpResponse response, DateTime expires)
        {
            response.Cookies.Append("Bearer", token,
            new CookieOptions()
            {
                Expires = expires,
                Secure = true,
                IsEssential = true
            });
        }

        public static IServiceCollection AddIdentityServices(this IServiceCollection services,
            IConfiguration config)
        {
            string? jwtSecret = config["JwtSecret"].InjectEnvironmentVariables();
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
                    opts.RequireHttpsMetadata = true;
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
                        },
                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            context.Response.Redirect("/account/login");
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
