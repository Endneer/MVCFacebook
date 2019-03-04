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
            if (signInManager.IsSignedIn(User))
            {
                ApplicationUser loggedinUser = context.Users.FirstOrDefault( U => U.Id == signInManager.UserManager.GetUserId(User));
                if (signInManager.UserManager.IsInRoleAsync(loggedinUser , "Admin").Result)
                {
                    return RedirectToAction("Index" , "Admin");       
                }
                return RedirectToAction("Home");
            }

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
