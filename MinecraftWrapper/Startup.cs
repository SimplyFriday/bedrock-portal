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
using MinecraftWrapper.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MinecraftWrapper.Models;
using MinecraftWrapper.Services;
using System.ComponentModel.Design;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace MinecraftWrapper
{
    public class Startup
    {
        public Startup ( IConfiguration configuration )
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices ( IServiceCollection services )
        {
            services.Configure<CookiePolicyOptions> ( options =>
              {
                  // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                  options.CheckConsentNeeded = context => true;
                  options.MinimumSameSitePolicy = SameSiteMode.None;
              } );

            services.AddDbContext<ApplicationDbContext> ( options =>
                  {
                      options.UseSqlServer (
                          Configuration.GetConnectionString ( "DefaultConnection" ) );
                  }, contextLifetime: ServiceLifetime.Transient, optionsLifetime: ServiceLifetime.Transient );


            services.AddDefaultIdentity<IdentityUser> (options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 4;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireDigit = false;
            } )
                .AddEntityFrameworkStores<ApplicationDbContext> ();

            services.AddMvc ().SetCompatibilityVersion ( CompatibilityVersion.Version_2_1 )
                .AddRazorPagesOptions ( options =>
                {
                    options.AllowAreas = true;
                    options.Conventions.AuthorizeAreaFolder ( "Identity", "/Account/Manage" );
                    options.Conventions.AuthorizeAreaPage ( "Identity", "/Account/Logout" );
                } );

            services.ConfigureApplicationCookie ( options =>
            {
                options.LoginPath = $"/Identity/Account/Login";
                options.LogoutPath = $"/Identity/Account/Logout";
                options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
            } );

            services.Configure<ApplicationSettings> ( Configuration.GetSection ( "ApplicationSettings" ) );

            services.AddTransient<UserRepository> ();
            services.AddTransient<SystemRepository> ();
            services.AddTransient<IEmailSender, SendGridSender> ();

            services.AddSingleton<ConsoleApplicationWrapper> ();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure ( IApplicationBuilder app, IHostingEnvironment env )
        {
            if ( env.IsDevelopment () )
            {
                app.UseDeveloperExceptionPage ();
                app.UseDatabaseErrorPage ();
            }
            else
            {
                app.UseExceptionHandler ( "/Home/Error" );
                app.UseHsts ();
            }

            app.UseHttpsRedirection ();
            app.UseStaticFiles ();
            app.UseCookiePolicy ();

            app.UseAuthentication ();

            app.UseMvc ( routes =>
              {
                  routes.MapRoute (
                      name: "default",
                      template: "{controller=Home}/{action=Index}/{id?}" );
              } );
        }
    }
}
