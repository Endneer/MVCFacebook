using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MVCFacebook.Data;
using MVCFacebook.Models;
using MVCFacebook.Models.AccountViewModels;

namespace MVCFacebook.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        //private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;

        public AdminController(
            ApplicationDbContext _context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            //IEmailSender emailSender,
            ILogger<AdminController> logger)
        {
            context = _context;
            _userManager = userManager;
            _signInManager = signInManager;
            //_emailSender = emailSender;
            _logger = logger;
        }

        public IActionResult Index()
        {
            //searchResult = null;
            return View(context.Users.ToList());

        }

        [HttpPost]
        public IActionResult Index(string value)
        {
            if (value == null)
                return PartialView("_AdminUserList", context.Users.ToList());
            else
            {
                var searchResult = context.Users.Where(u => string.Concat(u.UserName, u.FirstName, u.LastName).Contains(value)).ToList();
                return PartialView("_AdminUserList", searchResult);
            }
            //return View(context.Users.Where(u => string.Concat(u.UserName, u.FirstName, u.LastName).Contains(value)).ToList());
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult CreateUser(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        [HttpPost]
        public IActionResult Block(string id)
        {
            ApplicationUser user = context.Users.FirstOrDefault(U => U.Id == id);
            user.State = AccountState.Blocked;
            context.SaveChanges();

            //   return RedirectToAction("Index");
            return PartialView("_User", user);

        }
        [HttpPost]
        public IActionResult Unblock(string id)
        {
            ApplicationUser user = context.Users.FirstOrDefault(U => U.Id == id);
            user.State = AccountState.Active;
            context.SaveChanges();

            return PartialView("_User", user);
            //  return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FstName,
                    LastName = model.LstName,
                    Gender = model.Gender,
                    Bio = model.About,
                    BirthDate = model.Birthday

                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {

                    if (model.Admin == false)
                        await _userManager.AddToRoleAsync(user, "Member");
                    else
                        await _userManager.AddToRoleAsync(user, "Admin");

                    return RedirectToAction("Index");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

    }
}