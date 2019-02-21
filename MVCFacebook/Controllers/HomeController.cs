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
        public IActionResult Profile(int? id)
        {
            if (id == null)
            {
                //logged in user
                var loggedInUserID = signInManager.UserManager.GetUserId(User);
                var friends = context.Users.Where(u => u.Id == loggedInUserID).FirstOrDefault().Friends;
            }
            else
            {
                //user profile with the entered id
            }
            return View();
        }
    }
}
