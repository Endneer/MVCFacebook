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
            await userManager.CreateAsync(new ApplicationUser()
            {
                UserName = "Test1@Email.com",
                Email = "Test1@Email.com"
            }
            , "GGHHgghh.123");

            var x = context.Users.FirstOrDefault();

            await userManager.CreateAsync(new ApplicationUser()
            {
                UserName = "Test2@Email.com",
                Email = "Test2@Email.com"
            }
            , "GGHHgghh.123");

            await userManager.CreateAsync(new ApplicationUser()
            {
                UserName = "Test3@Email.com",
                Email = "Test3@Email.com"
            }
            , "GGHHgghh.123");

            

            await userManager.CreateAsync(new ApplicationUser()
            {
                UserName = "Test4@Email.com",
                Email = "Test4@Email.com"
            }
            , "GGHHgghh.123");

            await userManager.CreateAsync(new ApplicationUser()
            {
                UserName = "Test5@Email.com",
                Email = "Test5@Email.com"
            }
            , "GGHHgghh.123");


            //context.Users.FirstOrDefault(U => U.UserName == "Test1@Email.com").requestFriendship(context, context.Users.FirstOrDefault(U => U.UserName == "Test2@Email.com"));


            await context.SaveChangesAsync();
        }
    }
}
