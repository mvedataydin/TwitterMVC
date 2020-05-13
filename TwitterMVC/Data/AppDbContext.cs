using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitterMVC.Models;

namespace TwitterMVC.Data
{
    // IdentityDbContext contains all the user tables
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            :base(options)
        {

        }
        public DbSet<Tweet> Tweets { get; set; }
        public DbSet<TweetLike> TweetLikes { get; set; }
        public DbSet<UserFollow> UserFollows { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Tweets)
                .WithOne(t => t.User);

            modelBuilder.Entity<Tweet>()
                .HasOne<ApplicationUser>(t => t.User)
                .WithMany(u => u.Tweets);

            modelBuilder.Entity<TweetLike>()
                .HasKey(tl => new { tl.TweetId, tl.UserId });

            modelBuilder.Entity<TweetLike>()
                .HasOne<Tweet>(tl => tl.Tweet)
                .WithMany(t => t.TweetLikes)
                .HasForeignKey(tl => tl.TweetId);

            modelBuilder.Entity<TweetLike>()
                .HasOne<ApplicationUser>(tl => tl.User)
                .WithMany(u => u.TweetLikes)
                .HasForeignKey(tl => tl.UserId);


            modelBuilder.Entity<UserFollow>()
                .HasKey(k => new { k.UserId, k.FollowerId });

            modelBuilder.Entity<UserFollow>()
                .HasOne(l => l.User)
                .WithMany(a => a.Followers)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserFollow>()
                .HasOne(l => l.Follower)
                .WithMany(a => a.Following)
                .HasForeignKey(l => l.FollowerId);

        }

    }
}
