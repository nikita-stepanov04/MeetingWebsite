using MeetingWebsite.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetingWebsite.Infrastracture.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> opts)
            : base(opts) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Interest> Interests => Set<Interest>();
        public DbSet<Image> Images => Set<Image>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(u => u.Friends)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "UserFriend",
                    u => u
                        .HasOne<User>()
                        .WithMany()
                        .HasForeignKey("UserId"),
                    f => f
                        .HasOne<User>()
                        .WithMany()
                        .HasForeignKey("FriendId"));

            modelBuilder.Entity<Interest>()
                .HasMany(i => i.Users)
                .WithMany(u => u.Interests)
                .UsingEntity<Dictionary<string, object>>(
                    "UserInterest",
                    i => i
                        .HasOne<User>()
                        .WithMany()
                        .HasForeignKey("InterestId"),
                    u => u
                        .HasOne<Interest>()
                        .WithMany()
                        .HasForeignKey("UserId"));
            modelBuilder.Entity<User>()
                .Ignore(u => u.ImageLink);
        }
    }
}
