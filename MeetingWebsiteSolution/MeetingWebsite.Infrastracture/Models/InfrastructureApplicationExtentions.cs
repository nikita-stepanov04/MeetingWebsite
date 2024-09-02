using MeetingWebsite.Infrastracture.Models.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingWebsite.Infrastracture.Models
{
    public static class InfrastructureApplicationExtentions
    {
        public static IApplicationBuilder DatabaseMigrate(this IApplicationBuilder builder)
        {
            var serviceProvider = builder.ApplicationServices.CreateScope().ServiceProvider;
            var dbContext = serviceProvider.GetRequiredService<DataContext>();
            var identityContext = serviceProvider.GetRequiredService<IdentityContext>();

            if (dbContext.Database.GetPendingMigrations().Any())
            {
                dbContext.Database.Migrate();
            }
            if (identityContext.Database.GetAppliedMigrations().Any())
            {
                identityContext.Database.Migrate();
            }
            return builder;
        }
    }
}
