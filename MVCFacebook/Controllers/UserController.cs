using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCFacebook.Data;
using MVCFacebook.Models;

namespace MVCFacebook.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private ApplicationDbContext context;
        private SignInManager<ApplicationUser> signInManager;
        UserManager<ApplicationUser> UM;
        public UserController(ApplicationDbContext _context, SignInManager<ApplicationUser> _signInManager, UserManager<ApplicationUser> um)
        {
            context = _context;
            UM = um;
            signInManager = _signInManager;
        }

        public IActionResult Settings()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            if (signInManager.IsSignedIn(User))
            {
                ApplicationUser loggedinUser = context.Users.FirstOrDefault(U => U.Id == signInManager.UserManager.GetUserId(User));
                if (signInManager.UserManager.IsInRoleAsync(loggedinUser, "Admin").Result)
                {
                    return RedirectToAction("Index", "Admin");
                }
                return RedirectToAction("Home");
            }

            return View();
        }

        public IActionResult Home()
        {
            ApplicationUser loggedUser = context.Users.FirstOrDefault(x => x.Id == UM.GetUserId(HttpContext.User));
            loggedUser.loadFriendships(context);
            var friends = loggedUser.Friends.Select(x => x.Id);
            var postBag = (context.Posts.Include(p => p.Comments).Include(p => p.Likes).Include("Likes.LikingUser")
                .Where(x => (x.Creator.Id ==
                        UM.GetUserId(HttpContext.User)
                        || friends.Contains(x.Creator.Id))
                                && x.State == PostState.Active
                        ).OrderByDescending(y => y.CreationDate)).ToList<Post>();

            return View(postBag);
        }


        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Profile(string userName)
        {
            ApplicationUser user = context.Users.Include(u => u.Posts).Include("Posts.Comments").Include("Posts.Likes").FirstOrDefault(U => U.UserName == userName);
            user.Posts = user.Posts.Where(p => p.State == PostState.Active).ToList<Post>();
            if (user != null)
            {
                user.loadFriendships(context);
                return View(user);
            }
            else
                return RedirectToAction(nameof(Index));

        }


        #region Image Handling
        [HttpPost]
        public IActionResult UploadImage(IList<IFormFile> files, String id)
        {
            IFormFile uploadedImage = files.FirstOrDefault();

            if (uploadedImage.ContentType.ToLower().StartsWith("image/"))
            {
                MemoryStream ms = new MemoryStream();
                uploadedImage.OpenReadStream().CopyTo(ms);

                ApplicationUser user = context.Users.FirstOrDefault(U => U.Id == id);
                user.Image = ms.ToArray();
                user.ContentType = uploadedImage.ContentType;

                context.SaveChanges();
            }
            return PartialView("_UserImage", id);
        }

        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public FileStreamResult ViewImage(String id)
        {
            string realId = id.Split("_")[0];
            ApplicationUser user = context.Users.FirstOrDefault(U => U.Id == realId);
            MemoryStream ms = new MemoryStream(user.Image);
            return new FileStreamResult(ms, user.ContentType);
        }
        #endregion

        #region Post / Comment Operations
        [HttpPost]
        public IActionResult addPost(string post,string source)
        {
            ApplicationUser loggedUser = context.Users.FirstOrDefault(x => x.Id == UM.GetUserId(HttpContext.User));

            if (post != null && post.Length > 0)
            {
                // var Creator = context.Users.FirstOrDefault(x => x.Id == signInManager.UserManager.GetUserId(HttpContext.User)); 

                loggedUser.createPost(post, context);
                loggedUser.loadFriendships(context);
            }
            var friends = loggedUser.Friends.Select(x => x.Id);
            List<Post> postBag;
            ;
            if (source.StartsWith("/User/"))
            {
                postBag = (context.Posts.Include(p => p.Comments).Include(p => p.Likes).Include("Likes.LikingUser")
                                .Where(x => (x.Creator.Id ==
                            UM.GetUserId(HttpContext.User)
                        || friends.Contains(x.Creator.Id))
                                     && x.State == PostState.Active
                                    ).OrderByDescending(y => y.CreationDate)).ToList<Post>();
            }
            else
            {
                postBag = (context.Posts.Include(p => p.Comments).Include(p => p.Likes).Include("Likes.LikingUser")
                                .Where(x => x.Creator.Id ==
                                         UM.GetUserId(HttpContext.User)
                                            && x.State == PostState.Active
                                             ).OrderByDescending(y => y.CreationDate)).ToList<Post>();
            }
           
            return PartialView("_listPosts", postBag);//ajax
        }

        [HttpPost]
        public IActionResult DeletePost(int post, string source)
        {
            var usr = context.Users.FirstOrDefault(x => x.Id == signInManager.UserManager.GetUserId(HttpContext.User));
            var myPost = context.Posts.FirstOrDefault(p => p.ID == post);
            usr.deletePost(myPost, context);
            usr.loadFriendships(context);
            var friends = usr.Friends.Select(x => x.Id);
            ;
            List<Post> postBag;
            if (source.StartsWith("/User/"))
            {
                postBag = (context.Posts.Include(p => p.Comments).Include(p => p.Likes).Include("Likes.LikingUser")
                                .Where(x => (x.Creator.Id ==
                            UM.GetUserId(HttpContext.User)
                        || friends.Contains(x.Creator.Id))
                                     && x.State == PostState.Active
                                    ).OrderByDescending(y => y.CreationDate)).ToList<Post>();
            }
            else
            {
                postBag = (context.Posts.Include(p => p.Comments).Include(p => p.Likes).Include("Likes.LikingUser")
                                .Where(x => x.Creator.Id ==
                                         UM.GetUserId(HttpContext.User)
                                            && x.State == PostState.Active
                                             ).OrderByDescending(y => y.CreationDate)).ToList<Post>();
            }
            return PartialView("_listPosts", postBag);
        }

        [HttpPost]
        public IActionResult addComment(string commentText, int postId)
        {
            var usr = context.Users.FirstOrDefault(x => x.Id == signInManager.UserManager.GetUserId(HttpContext.User));
            var post = context.Posts.FirstOrDefault(p => p.ID == postId);
            usr.commentOnPost(post, commentText, context);
            var postBag = context.Posts.Include(p => p.Comments).Include(p => p.Likes).Include("Likes.LikingUser").FirstOrDefault(p => p.ID == postId);
            return PartialView("_listComments", postBag);
        }
        [HttpPost]
        public IActionResult DeleteComment(int commentId, int postId)
        {

            var usr = context.Users.FirstOrDefault(x => x.Id == signInManager.UserManager.GetUserId(HttpContext.User));
            var post = context.Posts.FirstOrDefault(p => p.ID == postId);
            context.Entry(post).Collection(u => u.Comments).Load();
            var myComment = post.Comments.FirstOrDefault(p => p.ID == commentId);
            usr.deleteComment(myComment, context);
            var postBag = context.Posts.Include(p => p.Comments).Include(p => p.Likes).Include("Likes.LikingUser").FirstOrDefault(p => p.ID == postId);
            return PartialView("_listComments", postBag);
            // return RedirectToAction("home");
        }
        [HttpPost]
        public IActionResult toggleLike(int post)
        {
            var usr = context.Users.FirstOrDefault(x => x.Id == signInManager.UserManager.GetUserId(HttpContext.User));
            var myPost = context.Posts.FirstOrDefault(p => p.ID == post);
            usr.toggleLike(myPost, context);
            var postBag = context.Posts.Include(p => p.Comments).Include(p => p.Likes).Include("Likes.LikingUser").FirstOrDefault(p => p.ID == post);
            return PartialView("_likes", postBag);
            //return RedirectToAction("home");
        }
        #endregion

        #region Friendship Operations
        [HttpPost]
        public IActionResult SendFriendRequest(string Id)
        {
            var user = context.Users.FirstOrDefault(a => a.Id == Id);
            var LoggedInUser = context.Users.FirstOrDefault(a => a.Id == signInManager.UserManager.GetUserId(User));

            LoggedInUser.requestFriendship(context, user);

            return PartialView("_FriendshipButton", user);
        }

        [HttpPost]
        public IActionResult RemoveFriend(string Id, string returnPartial)
        {
            ApplicationUser LoggedInUser = context.Users.FirstOrDefault(a => a.Id == signInManager.UserManager.GetUserId(User));

            LoggedInUser.loadFriendships(context);

            LoggedInUser.FriendRequestsRecieved.Remove(LoggedInUser.FriendRequestsRecieved.FirstOrDefault(F => F.User1ID == Id));
            LoggedInUser.FriendRequestsSent.Remove(LoggedInUser.FriendRequestsSent.FirstOrDefault(F => F.User2ID == Id));

            context.SaveChanges();


            ApplicationUser user = context.Users.FirstOrDefault(u => u.Id == Id);
            user.loadFriendships(context);

            if (returnPartial == "FButton")
                return PartialView("_FriendshipButton", user);
            else
                return PartialView("_Friends", LoggedInUser);
        }

        [HttpPost]
        public IActionResult AcceptFriendRequest(string id, string returnPartial)
        {

            string loggedInUserID = signInManager.UserManager.GetUserId(User);
            var loggedInUser = context.Users.FirstOrDefault(u => u.Id == loggedInUserID);

            loggedInUser.loadFriendships(context);

            loggedInUser.confirmFriendship(context,
                loggedInUser.FriendRequestsRecieved.FirstOrDefault(u => u.User1ID == id));

            ApplicationUser user = context.Users.FirstOrDefault(u => u.Id == id);
            user.loadFriendships(context);

            if (returnPartial == "FButton")
                return PartialView("_FriendshipButton", user);
            else
                return PartialView("_Friends", loggedInUser);

        }
        #endregion

    }
}
