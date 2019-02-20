using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Drawing;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCFacebook.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public Gender Gender { get; set; }
        public AccountState State { get; set; }
        public String Bio { get; set; }
        public DateTime BirthDate { get; set; }

        public virtual ICollection<Friendship> FriendRequestsSent { get; set; }
        public virtual ICollection<Friendship> FriendRequestsRecieved { get; set; }
        public virtual ICollection<UserComment> Comments { get; set; }
        public virtual ICollection<UserLike> Likes { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
        
        public ApplicationUser() {
            Comments = new HashSet<UserComment>();
            Likes = new HashSet<UserLike>();
            FriendRequestsSent = new HashSet<Friendship>();
            FriendRequestsRecieved = new HashSet<Friendship>();
        }

        [NotMapped]
        public ICollection<ApplicationUser> Friends { get =>
                FriendRequestsSent.Where(F => !F.Pending).Select(F => F.User2)
                .Union(FriendRequestsRecieved.Where(F => !F.Pending).Select(F => F.User1))
                .ToList();
                }

        public void loadFriendships(Data.ApplicationDbContext context)
        {
            context.Entry(this).Collection(u => u.FriendRequestsRecieved).Load();
            foreach (var F in FriendRequestsRecieved)
            {
                context.Entry(F).Reference(f => f.User1).Load();
            }
            context.Entry(this).Collection(u => u.FriendRequestsSent).Load();
            foreach (var F in FriendRequestsSent)
            {
                context.Entry(F).Reference(f => f.User2).Load();
            }
        }

        public bool requestFriendship(Data.ApplicationDbContext context,ApplicationUser user) {
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

        public bool confirmFriendship(Data.ApplicationDbContext context,Friendship request)
        {
            if (getPendingFriendRequests(context).Contains(request))
            {
                request.Pending = false;
                context.SaveChanges();
                return true;
            }
            return false;
        }


        public void denyFriendship(Data.ApplicationDbContext context ,Friendship request)
        {
            loadFriendships(context);
            FriendRequestsRecieved.Remove(request);
            context.SaveChanges();
        }

        public void createPost(String postText, Data.ApplicationDbContext context)
        {
            Posts.Add(new Post() {
                Text = postText,
                CreationDate = DateTime.Now,
                Creator = this,               
            });
            context.SaveChanges();
        }

        public void deletePost(Post tobeDeleted, Data.ApplicationDbContext context)
        {
            context.Entry(this).Collection(u => u.Posts).Load();
            Posts.Remove(tobeDeleted);
            context.SaveChanges();
        }

        public bool commentOnPost(Post p, Data.ApplicationDbContext context, String commentText)
        {
            if (context.Posts.Contains(p))
            {
                p.Comments.Add(new UserComment() {
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

        public bool deleteComment(UserComment comment,Data.ApplicationDbContext context)
        {
            context.Entry(comment).Reference(c => c.CommentingUser).Load();
            if (comment.CommentingUser.Id == Id)
            {
                context.Entry(comment).Reference(c => c.CommentedPost).Load();
                comment.CommentedPost.Comments.Remove(comment);
                context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool toggleLike(Post post,Data.ApplicationDbContext context)
        {
            context.Entry(post).Collection(p => p.Likes).Load();
            bool found = false;
            foreach(UserLike like in post.Likes)
            {
                if(like.UserID == Id)
                {
                    post.Likes.Remove(like);
                    found = true;
                }
            }

            if (!found)
                post.Likes.Add(new UserLike() {
                    LikingUser = this,
                    LikedPost = post
                });
            context.SaveChanges();
            return found;
        }

    }
}
