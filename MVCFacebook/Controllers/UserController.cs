using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
        
        public UserController(ApplicationDbContext _context, SignInManager<ApplicationUser> _signInManager)
        {
            context = _context;
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
            
            ///////

            string modelUserID;
            if (userName == null)
            {
                //logged in user
                modelUserID = signInManager.UserManager.GetUserId(User);
            }

            else
            {
                //user profile with the entered id
                modelUserID = context.Users.FirstOrDefault(u => u.UserName == userName).Id;
            }
            ApplicationUser modelUser = context.Users.FirstOrDefault(u => u.Id == modelUserID);


            if (userName == null) //Viewing my profile
            {
                var UsersRequestingFriendship = modelUser.getPendingFriendRequests(context).Select(u => u.User1).ToList();
                ViewBag.UsersRequestingFriendship = UsersRequestingFriendship;
                ViewBag.LoggedInUser = modelUser;
            }
            else //Viewing someone else profile
            {
                ViewBag.LoggedInUser = null;

                ViewBag.UsersRequestingFriendship = new List<ApplicationUser>();
            }
            var friends = modelUser.Friends;


            #region add dummy friendship between logged in user and test5 to test friends partial veiw
            try
            {
                modelUser.requestFriendship(context, context.Users.FirstOrDefault(i => i.UserName == "Test5@Email.com"));

                context.Users.FirstOrDefault(i => i.UserName == "Test5@Email.com").confirmFriendship(context,
                    context.Users.FirstOrDefault(i => i.UserName == "Test5@Email.com").FriendRequestsRecieved.FirstOrDefault());



                context.Users.FirstOrDefault(u => u.UserName == "Test4@Email.com").requestFriendship(context, modelUser);
                context.Users.FirstOrDefault(u => u.UserName == "Test3@Email.com").requestFriendship(context, modelUser);
            }
            catch (Exception)
            {

            }
            #endregion

            return View(modelUser);
        }

        public IActionResult addPost(Post po)
        {
            if (po.Text != null && po.Text.Length > 0)
            {
                po.CreationDate = DateTime.Now;
                po.Creator = context.Users.FirstOrDefault(x => x.Id == signInManager.UserManager.GetUserId(HttpContext.User));
                context.Posts.Add(po);
                context.SaveChanges();

            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult SendFriendRequest(string Id)
        {
            var user = context.Users.FirstOrDefault(a => a.Id == Id);
            var LoggedInUser = context.Users.FirstOrDefault(a => a.Id == signInManager.UserManager.GetUserId(User));

            if (LoggedInUser.getPendingFriendRequests(context).Contains(LoggedInUser.FriendRequestsRecieved.FirstOrDefault(a => a.User1ID == user.Id)))
            {
                LoggedInUser.confirmFriendship(context, LoggedInUser.getPendingFriendRequests(context).FirstOrDefault(u => u.User1ID == user.Id));
            }
            else if (user.getPendingFriendRequests(context).Contains(LoggedInUser.FriendRequestsRecieved.FirstOrDefault(a => a.User2ID == LoggedInUser.Id)))
            {
                user.confirmFriendship(context, user.getPendingFriendRequests(context).FirstOrDefault(u => u.User2ID == LoggedInUser.Id));
            }
            else if (LoggedInUser.Friends.Contains(user))
            {
                LoggedInUser.Friends.Remove(user);
            }
            else
            {
                LoggedInUser.requestFriendship(context, user);
            }

            return RedirectToAction("Profile", new { userName = user.UserName });
        }

        [HttpPost]
        public IActionResult AcceptFriendRequest(string id)
        {

            string loggedInUserID = signInManager.UserManager.GetUserId(User);
            var loggedInUser = context.Users.FirstOrDefault(u => u.Id == loggedInUserID);

            loggedInUser.loadFriendships(context);

            loggedInUser.confirmFriendship(context,
                loggedInUser.FriendRequestsRecieved.FirstOrDefault(u => u.User1ID == id));

            return RedirectToAction("Profile");
        }

        public IActionResult RejectFriendRequest(string id)
        {

            string loggedInUserID = signInManager.UserManager.GetUserId(User);
            var loggedInUser = context.Users.FirstOrDefault(u => u.Id == loggedInUserID);

            loggedInUser.loadFriendships(context);

            loggedInUser.denyFriendship(context,
                loggedInUser.FriendRequestsRecieved.FirstOrDefault(u => u.User1ID == id));

            return RedirectToAction("Profile");
        }

    }
}
