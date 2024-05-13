using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

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
                    opts.UseSqlServer(connectionString, 
                    builder => builder.MigrationsAssembly("MeetingWebsite.Infrastracture")));
            }
            else
            {
                throw new Exception("DbConnection string is not defined");
            }            
            return services;
        }
    }
}
