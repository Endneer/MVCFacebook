using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MVCFacebook.Models;

namespace MVCFacebook.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Friendship>().HasKey(c => new { c.User1ID, c.User2ID });
            builder.Entity<UserLike>().HasKey(c => new { c.UserID, c.PostID });

            builder.Entity<UserLike>()
                .HasOne(L => L.LikingUser)
                .WithMany(U => U.Likes)
                .HasForeignKey(L => L.UserID);
            builder.Entity<UserLike>()
                .HasOne(L => L.LikedPost)
                .WithMany(U => U.Likes)
                .HasForeignKey(L => L.PostID);

            builder.Entity<UserComment>()
                .HasOne(L => L.CommentingUser)
                .WithMany(U => U.Comments)
                .HasForeignKey(L => L.UserID);
            builder.Entity<UserComment>()
                .HasOne(L => L.CommentedPost)
                .WithMany(U => U.Comments)
                .HasForeignKey(L => L.PostID);

            builder.Entity<Friendship>()
                .HasOne(pt => pt.User1)
                .WithMany(p => p.FriendRequestsSent)
                .HasForeignKey(pt => pt.User1ID).
                OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Friendship>()
                .HasOne(pt => pt.User2)
                .WithMany(p => p.FriendRequestsRecieved)
                .HasForeignKey(pt => pt.User2ID).
                OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(builder);
        }

        public virtual DbSet<Post> Posts { get; set; }
    }
}
