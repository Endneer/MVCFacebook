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
    public class HomeController : Controller
    {

        private ApplicationDbContext context;
        private SignInManager<ApplicationUser> signInManager;

        public HomeController(ApplicationDbContext _context, SignInManager<ApplicationUser> _signInManager)
        {
            context = _context;
            signInManager = _signInManager;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [Authorize]
        public IActionResult Profile(string userName)
        {


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
            var friends = modelUser.Friends;


            #region add dummy friendship between logged in user and test5 to test friends partial veiw
            try
            {
                modelUser.requestFriendship(context, context.Users.FirstOrDefault(i => i.UserName == "Test5@Email.com"));

                context.Users.FirstOrDefault(i => i.UserName == "Test5@Email.com").confirmFriendship(context,
                    context.Users.FirstOrDefault(i => i.UserName == "Test5@Email.com").FriendRequestsRecieved.FirstOrDefault());
            }
            catch (Exception)
            {

                throw;
            }
            #endregion


            return View(modelUser);
        }
    }
}
