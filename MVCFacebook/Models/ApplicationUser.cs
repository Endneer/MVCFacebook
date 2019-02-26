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
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public Gender Gender { get; set; }
        public AccountState State { get; set; } = AccountState.Active;
        public String Bio { get; set; }
        public DateTime BirthDate { get; set; }
        public byte[] Image { get; set; }

        public virtual ICollection<Friendship> FriendRequestsSent { get; set; }
        public virtual ICollection<Friendship> FriendRequestsRecieved { get; set; }
        public virtual ICollection<UserComment> Comments { get; set; }
        public virtual ICollection<UserLike> Likes { get; set; }
        public virtual ICollection<Post> Posts { get; set; }

        public ApplicationUser()
        {
            Comments = new HashSet<UserComment>();
            Likes = new HashSet<UserLike>();
            FriendRequestsSent = new HashSet<Friendship>();
            FriendRequestsRecieved = new HashSet<Friendship>();
        }

        [NotMapped]
        public ICollection<ApplicationUser> Friends
        {
            get =>
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
            context.Entry(this).Collection(u => u.Posts).Load();
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
                context.Entry(tobeDeleted).Collection(p => p.Likes).Load();
                context.Entry(tobeDeleted).Collection(p => p.Comments).Load();
                tobeDeleted.Likes.Clear();
                tobeDeleted.Comments.Clear();
                Posts.Remove(tobeDeleted);
                context.Posts.Remove(tobeDeleted);
                context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool commentOnPost(Post p, Data.ApplicationDbContext context, String commentText)
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

    }
}
