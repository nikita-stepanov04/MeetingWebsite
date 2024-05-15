using MeetingWebsite.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MeetingWebsite.Infrastracture.Models.Identity
{
    public class IdentityContext : IdentityDbContext<AppUser, AppRole, long>
    {
        public IdentityContext(DbContextOptions<IdentityContext> opts)
            : base(opts) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Ignore<User>();
        }
    }
}
