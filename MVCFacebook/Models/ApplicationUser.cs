﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Drawing;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MVCFacebook.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public Gender Gender { get; set; }
        public AccountState State { get; set; } = AccountState.Active;
        public String Bio { get; set; }
        public DateTime BirthDate { get; set; }
        public byte[] Image { get; set; }
        public String ContentType { get; set; }

        public virtual ICollection<Friendship> FriendRequestsSent { get; set; }
        public virtual ICollection<Friendship> FriendRequestsRecieved { get; set; }
        public virtual ICollection<UserComment> Comments { get; set; }
        public virtual ICollection<UserLike> Likes { get; set; }
        public virtual ICollection<Post> Posts { get; set; }

        public ApplicationUser()
        {
            Comments = new HashSet<UserComment>();
            Likes = new HashSet<UserLike>();
            Posts = new HashSet<Post>();
            FriendRequestsSent = new HashSet<Friendship>();
            FriendRequestsRecieved = new HashSet<Friendship>();
        }

        [NotMapped]
        public ICollection<ApplicationUser> Friends
        {
            get =>
            FriendRequestsSent.Where(F => !F.Pending && F.User2 != null).Select(F => F.User2)
            .Union(FriendRequestsRecieved.Where(F => !F.Pending && F.User1 != null).Select(F => F.User1))
            .ToList();
        }

        public void loadFriendships(Data.ApplicationDbContext context)
        {
            context.Entry(this).Collection(u => u.FriendRequestsRecieved).Load();
            foreach (var F in FriendRequestsRecieved)
            {
                if (!(context.Entry(F).State == Microsoft.EntityFrameworkCore.EntityState.Detached))
                    context.Entry(F).Reference(f => f.User1).Load();
            }
            context.Entry(this).Collection(u => u.FriendRequestsSent).Load();
            foreach (var F in FriendRequestsSent)
            {
                if (!(context.Entry(F).State == Microsoft.EntityFrameworkCore.EntityState.Detached))
                    context.Entry(F).Reference(f => f.User2).Load();
            }
        }

        public bool requestFriendship(Data.ApplicationDbContext context, ApplicationUser user)
        {
            loadFriendships(context);
            if (Friends.Contains(user))
            {
                return false;
            }
            FriendRequestsSent.Add(new Friendship() { User1 = this, User2 = user, Pending = true });
            context.SaveChanges();
            return true;
        }

        public ICollection<Friendship> getPendingFriendRequests(Data.ApplicationDbContext context)
        {
            loadFriendships(context);
            return FriendRequestsRecieved.Where(F => F.Pending).ToList();
        }

        public bool confirmFriendship(Data.ApplicationDbContext context, Friendship request)
        {
            if (getPendingFriendRequests(context).Contains(request))
            {
                request.Pending = false;
                context.SaveChanges();
                return true;
            }
            return false;
        }


        public void denyFriendship(Data.ApplicationDbContext context, Friendship request)
        {
            loadFriendships(context);
            FriendRequestsRecieved.Remove(request);
            context.SaveChanges();
        }

        public void createPost(String postText, Data.ApplicationDbContext context)
        {

            Posts.Add(new Post()
            {
                Text = postText,
                CreationDate = DateTime.Now,
                Creator = this,
            });
            context.SaveChanges();
        }

        public bool deletePost(Post tobeDeleted, Data.ApplicationDbContext context)
        {
            context.Entry(this).Collection(u => u.Posts).Load();
            if (Posts.Any(P => P.ID == tobeDeleted.ID))
            {
                tobeDeleted.State = PostState.Deleted;
                context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool commentOnPost(Post p, String commentText, Data.ApplicationDbContext context)
        {

            ;
            if (context.Posts.Any(po => po.ID == p.ID))
            {
                context.Entry(p).Collection(post => post.Comments).Load();
                p.Comments.Add(new UserComment()
                {
                    CommentedPost = p,
                    CommentingUser = this,
                    CommentText = commentText,
                    CommentDate = DateTime.Now
                });
                context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool deleteComment(UserComment comment, Data.ApplicationDbContext context)
        {
            context.Entry(comment).Reference(c => c.CommentedPost).Load();
            var post = comment.CommentedPost;
            context.Entry(post).Reference(c => c.Creator).Load();
            if (comment.UserID == Id ||post.Creator.Id==Id)
            {
                comment.State = PostState.Deleted;
                context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool toggleLike(Post post, Data.ApplicationDbContext context)
        {
            context.Entry(post).Collection(p => p.Likes).Load();
            UserLike foundLike = post.Likes.FirstOrDefault(L => L.UserID == Id);

            if (foundLike == null)
                post.Likes.Add(new UserLike()
                {
                    LikingUser = this,
                    LikedPost = post
                });
            else
                post.Likes.Remove(foundLike);

            context.SaveChanges();
            return foundLike != null;
        }



        public ICollection<Post> getPosts(bool includeFriends, Data.ApplicationDbContext context)
        {
            if (includeFriends)
            {
                loadFriendships(context);
                return context.Posts.Include(p => p.Comments).Include(p => p.Likes).Include("Likes.LikingUser")
                    .Include("Comments.CommentingUser").Where(
                    p => (p.Creator.Id == Id || Friends.Select(F => F.Id).Contains(p.Creator.Id))
                    && p.State == PostState.Active
                    ).OrderByDescending(p => p.CreationDate).ToList();
            }

            return context.Posts.Include(p => p.Comments).Include(p => p.Likes).Include("Likes.LikingUser")
                    .Include("Comments.CommentingUser").Where(
                    p => p.Creator.Id == Id && p.State == PostState.Active
                    ).OrderByDescending(p => p.CreationDate).ToList();
        }

    }
}
