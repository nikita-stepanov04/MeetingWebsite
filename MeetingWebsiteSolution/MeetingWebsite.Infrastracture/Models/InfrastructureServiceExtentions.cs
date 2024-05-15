using MeetingWebsite.Infrastracture.Models.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MeetingWebsite.Infrastracture.Models
{
    public static class InfrastructureServiceExtentions
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services, ConfigurationManager config)
        {
            string? connectionString = config.GetConnectionString("DbConnection");
            if (connectionString != null)
            {
                services.AddDbContext<DataContext>(opts =>
                    opts.UseSqlServer(connectionString, opts
                        => opts.MigrationsAssembly("MeetingWebsite.Infrastracture")));

                services.AddDbContext<IdentityContext>(opts =>
                    opts.UseSqlServer(connectionString, opts
                        => opts.MigrationsAssembly("MeetingWebsite.Infrastracture")));
            }
            else
            {
                throw new Exception("DbConnection string is not defined");
            }            
            return services;
        }
    }
}
