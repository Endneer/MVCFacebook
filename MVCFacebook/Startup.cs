using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCFacebook.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MVCFacebook.Models;

namespace MVCFacebook
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env
            , UserManager<ApplicationUser> um, ApplicationDbContext context,
            RoleManager<IdentityRole> Rolemanager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {

                routes.MapRoute(
                    name: null,
                    template: "Profile/{userName}",
                    defaults: new { action="Profile" , controller ="User" });

                    routes.MapRoute(
                     name: null,
                     template: "Settings",
                     defaults: new { action = "Settings", controller = "User" });

                routes.MapRoute(
                     name: null,
                     template: "Home",
                     defaults: new { action = "Home", controller = "User" });

                routes.MapRoute(
                     name: null,
                     template: "ViewImage/{id}",
                     defaults: new { action = "ViewImage", controller = "User" });

                routes.MapRoute(
                     name: null,
                     template: "{action}",
                     defaults: new { controller = "User" });


                routes.MapRoute(
                     name: null,
                     template: "",
                     defaults: new { action = "Index", controller = "User" });

                routes.MapRoute(
                    name: "default",
                    template: "{controller=User}/{action=Index}/");
            });

            #region Seeding
            //Role initialization
            if (!(Rolemanager.RoleExistsAsync("Member")).Result)
                Rolemanager.CreateAsync(new IdentityRole()
                {
                    Name = "Member"
                });

            if (!(Rolemanager.RoleExistsAsync("Admin")).Result)
                Rolemanager.CreateAsync(new IdentityRole()
                {
                    Name = "Admin"
                });


            if (!context.Users.Any(U => U.UserName == "Admin@Impostagram.com"))
            {
                ApplicationUser admin = new ApplicationUser()
                {
                    UserName = "Admin@Impostagram.com",
                    Email = "Admin@Impostagram.com",
                };
                um.CreateAsync(admin , "GGHHgghh.123").Wait();
                um.AddToRoleAsync(admin,"Admin").Wait();
                context.SaveChanges();
            }

            #endregion

        }
    }
}
