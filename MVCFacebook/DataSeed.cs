using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MVCFacebook.Models;
using MVCFacebook.Data;
using Microsoft.EntityFrameworkCore;

namespace MVCFacebook
{
    public static class DataSeed
    {
        public static async Task SeedDatabase(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
          

        //    await userManager.CreateAsync(new ApplicationUser()
        //    {
        //        UserName = "Test5@Email.com",
        //        Email = "Test5@Email.com"
        //    }
        //    , "GGHHgghh.123");


        //    //context.Users.FirstOrDefault(U => U.UserName == "Test1@Email.com").requestFriendship(context, context.Users.FirstOrDefault(U => U.UserName == "Test2@Email.com"));


        //    await context.SaveChangesAsync();

        }
    }
}
