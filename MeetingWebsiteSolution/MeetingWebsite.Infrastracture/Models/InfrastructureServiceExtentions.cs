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
            string? dbConnection = config
                .GetConnectionString("DbConnection")
                .InjectEnvironmentVariables();            

            string? redisConnection = config
                .GetConnectionString("RedisConnection")
                .InjectEnvironmentVariables();

            if (dbConnection != null && redisConnection != null)
            {
                services.AddDbContext<DataContext>(opts => 
                    opts.UseNpgsql(dbConnection, opts
                        => opts.MigrationsAssembly("MeetingWebsite.Infrastracture")));

                services.AddDbContext<IdentityContext>(opts =>
                    opts.UseNpgsql(dbConnection, opts
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
                throw new Exception($"Connection strings are undefined," +
                    $" db connection string: {dbConnection}, redis connection string: {redisConnection}");
            }
            return services;
        }
    }
}
