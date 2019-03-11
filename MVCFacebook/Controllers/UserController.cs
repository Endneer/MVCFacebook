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


        #region Unsorted
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
            return View(loggedUser.getPosts(true, context));
        }


        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Profile(string userName)
        {
            ApplicationUser user = context.Users.Include(u => u.Posts).Include("Posts.Comments").Include("Posts.Comments.CommentingUser")
                .Include("Posts.Likes").Include("Posts.Likes.LikingUser")
                .FirstOrDefault(U => U.UserName == userName);
            if (user != null)
            {
                user.loadFriendships(context);
                return View(user);
            }
            else
                return RedirectToAction(nameof(Index));

        }

        [HttpGet]
        public IActionResult Search(string searchText)
        {

            searchText = searchText.Trim();
            var searchResult = UM.GetUsersInRoleAsync("Member").Result.
                Where(u => string.Concat(u.UserName, u.FirstName, u.LastName).Contains(searchText)
                          && (u.State == AccountState.Active)).ToList();

            return View(searchResult);
        }

        [HttpGet]
        public IActionResult EditInfo()
        {
            var user = context.Users.FirstOrDefault(u => u.Id == UM.GetUserId(User));

            Models.UserViewModels.EditInfoViewModel model = new Models.UserViewModels.EditInfoViewModel() {
                FirstName = user.FirstName,
                Lastname = user.LastName,
                Biography = user.Bio
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult EditInfo(Models.UserViewModels.EditInfoViewModel EditInfoModel)
        {            
            if (ModelState.IsValid)
            {

                var user = context.Users.FirstOrDefault(u => u.Id == UM.GetUserId(User));

                user.FirstName = EditInfoModel.FirstName;
                user.LastName = EditInfoModel.Lastname;
                user.Bio = EditInfoModel.Biography;
                context.SaveChanges();


                return RedirectToAction("Profile", new { userName = user.UserName });
            }

            return View("EditInfo", EditInfoModel);
        }

        [HttpPost]
        public IActionResult EditPassword(Models.UserViewModels.SettingsViewModel SettingsModel)
        {
            var user = context.Users.FirstOrDefault(u => u.Id == UM.GetUserId(User));
            if (ModelState.IsValid)
            {
                if (UM.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, SettingsModel.Old_Password) != PasswordVerificationResult.Success)
                {
                    ModelState.AddModelError(String.Empty, "Old Password does not match");
                    return View("Settings", SettingsModel);
                }

                if (SettingsModel.New_Password != SettingsModel.Confirm_Password)
                {
                    ModelState.AddModelError(String.Empty, "New password does not match confirmation");
                    return View("Settings", SettingsModel);
                }

                if (ModelState.IsValid)
                {
                    UM.ChangePasswordAsync(user, SettingsModel.Old_Password, SettingsModel.New_Password).Wait();
                    context.SaveChanges();
                }
            }
            else
            {
                return View("Settings", SettingsModel);
            }

            return RedirectToAction("Profile", new { userName = user.UserName });
        }
        #endregion

        #region Image Handling
        [HttpPost]
        public IActionResult UploadImage(IList<IFormFile> files, String id)
        {

            IFormFile uploadedImage = files.FirstOrDefault();

            if (uploadedImage != null && uploadedImage.ContentType.ToLower().StartsWith("image/"))
            {
                MemoryStream ms = new MemoryStream();
                uploadedImage.OpenReadStream().CopyTo(ms);

                ApplicationUser user = context.Users.FirstOrDefault(U => U.Id == id);
                user.Image = ms.ToArray();
                user.ContentType = uploadedImage.ContentType;

                context.SaveChanges();
            }

            return PartialView("_UserImage", context.Users.FirstOrDefault(u=>u.Id == id));
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
        public IActionResult addPost(string post, string source)
        {
            // Getting Logged in User
            ApplicationUser loggedUser = context.Users.FirstOrDefault(x => x.Id == UM.GetUserId(HttpContext.User));

            // Inserting Post if post text is valid
            if (post != null && post.Length > 0)
            {
                loggedUser.createPost(post, context);
            }

            if (source.StartsWith("/Profile/"))
            {
                String ownerUserName = source.Split('/')[2];
                ApplicationUser ownerUser = context.Users.FirstOrDefault(u => u.UserName == ownerUserName);
                return PartialView("_listPosts", ownerUser.getPosts(false, context));
            }
            return PartialView("_listPosts", loggedUser.getPosts(true, context));
        }
        [HttpPost]
        public IActionResult DeletePost(int post, string source)
        {

            var usr = context.Users.FirstOrDefault(x => x.Id == signInManager.UserManager.GetUserId(HttpContext.User));
            var myPost = context.Posts.FirstOrDefault(p => p.ID == post);
            usr.deletePost(myPost, context);

            if (source.StartsWith("/Profile/"))
            {
                String ownerUserName = source.Split('/')[2];
                ApplicationUser ownerUser = context.Users.FirstOrDefault(u => u.UserName == ownerUserName);
                return PartialView("_listPosts", ownerUser.getPosts(false, context));
            }
            return PartialView("_listPosts", usr.getPosts(true, context));
        }

        [HttpPost]
        public IActionResult addComment(string commentText, int postId)
        {

            if (commentText != null && commentText.Length > 0)
            {
                var usr = context.Users.FirstOrDefault(x => x.Id == signInManager.UserManager.GetUserId(HttpContext.User));
                var post = context.Posts.FirstOrDefault(p => p.ID == postId);
                usr.commentOnPost(post, commentText, context);
            }

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
                return PartialView("_UserInfo", user);
            else
                return PartialView("_UserInfo", LoggedInUser);
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
                return PartialView("_UserInfo", user);
            else
                return PartialView("_UserInfo", loggedInUser);

        }
        #endregion

    }
}
