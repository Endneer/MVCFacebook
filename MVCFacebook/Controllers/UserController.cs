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

        //
        //public IActionResult Index_old()
        //{
        //    ApplicationUser loggedUser = context.Users.FirstOrDefault(x => x.Id == signInManager.UserManager.GetUserId(HttpContext.User));
        //    loggedUser.loadFriendships(context);
        //    var friends = loggedUser.Friends.Select(x => x.Id);
        //    ViewBag.posts = (context.Posts.Where(x => x.Creator.Id == signInManager.UserManager.GetUserId(HttpContext.User) || friends.Contains(x.Creator.Id)).OrderByDescending(y => y.CreationDate));
        //    return View();
        //}

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Home()
        {
            ApplicationUser loggedUser = context.Users.FirstOrDefault(x => x.Id == UM.GetUserId(HttpContext.User));
            loggedUser.loadFriendships(context);
            var friends = loggedUser.Friends.Select(x => x.Id);
            ViewBag.posts = (context.Posts.Include(p=>p.Comments).Where(x => x.Creator.Id == UM.GetUserId(HttpContext.User) || friends.Contains(x.Creator.Id)).OrderByDescending(y => y.CreationDate));
            
            return View();
        }

        public IActionResult Settings()
        {
            return View();
        }

        [Authorize]
        public IActionResult Profile(string userName)
        {
            ApplicationUser user =  context.Users.FirstOrDefault(U=>U.UserName == userName);

            if (user != null) {
                user.loadFriendships(context);
                return View(user);
            }
            else
                return RedirectToAction(nameof(Index));
            
        }

        [HttpPost]
        public IActionResult UploadImage(IList<IFormFile> files,String id)
        {
            IFormFile uploadedImage = files.FirstOrDefault();

            if (uploadedImage.ContentType.ToLower().StartsWith("image/"))
            {
                MemoryStream ms = new MemoryStream();
                uploadedImage.OpenReadStream().CopyTo(ms);

                ApplicationUser user = context.Users.FirstOrDefault(U=> U.Id == id);
                user.Image = ms.ToArray();
                user.ContentType = uploadedImage.ContentType;
                
                context.SaveChanges();
            }
            return RedirectToAction("Profile/" + signInManager.UserManager.GetUserName(User));
        }

        [HttpGet]
        public FileStreamResult ViewImage(String id)
        {
            ApplicationUser user = context.Users.FirstOrDefault(U => U.Id == id);
            MemoryStream ms = new MemoryStream(user.Image);
            return new FileStreamResult(ms, user.ContentType);
        }
        [Authorize]
        public IActionResult addPost(Post po)
        {
            if (po.Text != null && po.Text.Length > 0)
            {
                po.CreationDate = DateTime.Now;
                po.Creator = context.Users.FirstOrDefault(x => x.Id == signInManager.UserManager.GetUserId(HttpContext.User));
                context.Posts.Add(po);
                context.SaveChanges();

            }
            return RedirectToAction("Home");
        }
        [Authorize]
        public IActionResult DeletePost(int post)
        {
            
            var usr = context.Users.FirstOrDefault(x => x.Id == signInManager.UserManager.GetUserId(HttpContext.User));
            var myPost = context.Posts.FirstOrDefault(p => p.ID==post);
            usr.deletePost(myPost, context);
            return RedirectToAction("Home");
        }
        [Authorize]
        public IActionResult addComment(UserComment comment)
        {
            var usr = context.Users.FirstOrDefault(x => x.Id == signInManager.UserManager.GetUserId(HttpContext.User));
            var post = context.Posts.FirstOrDefault(p => p.ID == comment.PostID);
            usr.commentOnPost(post, comment.CommentText,context);
            return RedirectToAction("Home");
        }
        [HttpPost]
        public IActionResult SendFriendRequest(string Id )
        {
            var user = context.Users.FirstOrDefault(a => a.Id == Id);
            var LoggedInUser = context.Users.FirstOrDefault(a => a.Id == signInManager.UserManager.GetUserId(User));

            LoggedInUser.requestFriendship(context, user);

            return RedirectToAction($"Profile/{user.UserName}");
        }

        [HttpPost]
        public IActionResult RemoveFriend(string Id)
        {
            ApplicationUser LoggedInUser = context.Users.FirstOrDefault(a => a.Id == signInManager.UserManager.GetUserId(User));

            LoggedInUser.loadFriendships(context);

            LoggedInUser.FriendRequestsRecieved.Remove(LoggedInUser.FriendRequestsRecieved.FirstOrDefault(F=>F.User1ID == Id));
            LoggedInUser.FriendRequestsSent.Remove(LoggedInUser.FriendRequestsSent.FirstOrDefault(F=>F.User2ID == Id));

            context.SaveChanges();

            return RedirectToAction($"Profile/{context.Users.FirstOrDefault(U=>U.Id == Id).UserName}");
        }

        [HttpPost]
        public IActionResult AcceptFriendRequest(string id,string returnUrl)
        {

            string loggedInUserID = signInManager.UserManager.GetUserId(User);
            var loggedInUser = context.Users.FirstOrDefault(u => u.Id == loggedInUserID);

            loggedInUser.loadFriendships(context);

            loggedInUser.confirmFriendship(context,
                loggedInUser.FriendRequestsRecieved.FirstOrDefault(u => u.User1ID == id));

            return RedirectToAction(returnUrl);
        }


    }
}
