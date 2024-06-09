using MeetingWebsite.Application.Interfaces;
using MeetingWebsite.Infrastracture.Models.Identity;
using MeetingWebsite.Infrastracture.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingWebsite.Infrastracture.Models
{
    public static class InfrastructureServiceExtentions
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services, ConfigurationManager config)
        {
            string? dbConnection = config.GetConnectionString("DbConnection");
            string? redisConnection = config.GetConnectionString("RedisConnection");
            if (dbConnection != null && redisConnection != null)
            {
                services.AddDbContext<DataContext>(opts =>
                    opts.UseSqlServer(dbConnection, opts
                        => opts.MigrationsAssembly("MeetingWebsite.Infrastracture")));

                services.AddDbContext<IdentityContext>(opts =>
                    opts.UseSqlServer(dbConnection, opts
                        => opts.MigrationsAssembly("MeetingWebsite.Infrastracture")));

                services.AddStackExchangeRedisCache(opts =>
                {
                    opts.Configuration = redisConnection;
                    opts.InstanceName = "MeetingWebsite_";
                });
                services.AddScoped<IDistributedMeetingCache, DistributedMeetingCache>();
            }
            else
            {
                throw new Exception("Connection strings are undefined");
            }
            return services;
        }
    }
}
