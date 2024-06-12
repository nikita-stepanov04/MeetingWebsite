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
        public DbSet<FriendshipRequest> FriendshipRequests => Set<FriendshipRequest>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Friends)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "UserFriend",
                    u => u
                        .HasOne<User>().WithMany()
                        .HasForeignKey("UserId"),
                    f => f
                        .HasOne<User>().WithMany()
                        .HasForeignKey("FriendId"));

            modelBuilder.Entity<User>()
                .HasMany(u => u.Interests)
                .WithMany(i => i.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserInterest",
                    j => j
                        .HasOne<Interest>().WithMany()
                        .HasForeignKey("InterestId"),
                    j => j
                        .HasOne<User>().WithMany()
                        .HasForeignKey("UserId"));

            modelBuilder.Entity<User>()
                .Ignore(u => u.ImageLink);

            modelBuilder.Entity<FriendshipRequest>()
                .HasKey(fr => fr.RequestId);

            modelBuilder.Entity<FriendshipRequest>()
                .HasOne(fr => fr.Sender).WithMany()
                .HasForeignKey(fr => fr.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FriendshipRequest>()
                .HasOne(fr => fr.Receiver).WithMany()
                .HasForeignKey(fr => fr.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
