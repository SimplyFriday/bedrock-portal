using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MinecraftWrapper.Data;
using MinecraftWrapper.Data.Constants;
using MinecraftWrapper.Data.Entities;
using MinecraftWrapper.Services;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

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
            ConfigureSerilog ();

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


            services.AddIdentity<ApplicationUser, IdentityRole> ( options =>
             {
                 options.Password.RequiredLength = 8;
                 options.Password.RequiredUniqueChars = 4;
                 options.Password.RequireLowercase = false;
                 options.Password.RequireNonAlphanumeric = false;
                 options.Password.RequireUppercase = false;
                 options.Password.RequireDigit = false;
             } )
                .AddDefaultTokenProviders ()
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

            services.AddAuthentication ()
                .AddGoogle ( options =>
                {
                    options.ClientId = Configuration[ "Authentication:Google:ClientId" ];
                    options.ClientSecret = Configuration[ "Authentication:Google:ClientSecret" ];
                } );

            // We're doing some crazy stuff with singletons, so basically everything is either transient or singleton.
            services.AddTransient<UserRepository> ();
            services.AddTransient<SystemRepository> ();
            services.AddTransient<IEmailSender, SendGridSender> ();
            services.AddTransient<WhiteListService> ();
            services.AddTransient<DiscordService> ();
            services.AddTransient<MinecraftStoreService> ();
            services.AddTransient<StoreRepository> ();
            services.AddTransient<ScheduledTaskRepository> ();
            services.AddTransient<BackupService> ();

            services.AddSingleton<StatusService> ();
            services.AddSingleton<ConsoleApplicationWrapper<MinecraftMessageParser>> ();
            services.AddSingleton<MinecraftMessageParser> ();
            services.AddSingleton<ScheduledTaskService> ();
        }

        private void ConfigureSerilog ()
        {
            var traceIdentifierColumn   =  new DataColumn ( "TraceIdentifier", typeof ( string ) );
            var urlColumn               =  new DataColumn ( "Url", typeof ( string ) );

            traceIdentifierColumn.MaxLength = 50;
            urlColumn.MaxLength = 450;

            var extraColumns = new List<DataColumn>
            {
                traceIdentifierColumn,
                urlColumn
            };

            var columnOptions = new ColumnOptions ();
            columnOptions.AdditionalDataColumns = extraColumns;

            Log.Logger = new LoggerConfiguration ()
                .MinimumLevel.Information ()
                .MinimumLevel.Override ( "Microsoft", LogEventLevel.Warning )
                .Enrich.FromLogContext ()
                .WriteTo.MSSqlServer ( Configuration.GetConnectionString ( "DefaultConnection" ), "EventLog", autoCreateSqlTable: true, columnOptions: columnOptions )
                .CreateLogger ();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure ( IApplicationBuilder app, IHostingEnvironment env, IServiceProvider provider )
        {
            UpdateDatabase ( app );

            app.UseExceptionHandler ( "/Home/Error" );

            if ( !env.IsDevelopment () )
            {
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

            CreateDefaultRoles ( provider ).Wait ();
            RegisterTasks ( provider );

            // Moved this to last because it takes a ton of resources
            var wrapper = provider.GetService<ConsoleApplicationWrapper<MinecraftMessageParser>> ();
            wrapper.Start ();
        }

        private static void UpdateDatabase ( IApplicationBuilder app )
        {
            using ( var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory> ()
                .CreateScope () )
            {
                using ( var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext> () )
                {
                    context.Database.Migrate ();
                }
            }
        }
        private void RegisterTasks ( IServiceProvider provider )
        {
            var taskService = provider.GetRequiredService<ScheduledTaskService> ();
            taskService.RegisterTasks ();
            taskService.Start ();
        }

        private async Task CreateDefaultRoles ( IServiceProvider serviceProvider )
        {
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var defaultRoles = new string[] { "Admin", "Moderator" };

            foreach ( var role in defaultRoles )
            {
                IdentityResult roleResult;
                var roleCheck = await RoleManager.RoleExistsAsync(role);

                if ( !roleCheck )
                {
                    //create the roles and seed them to the database
                    roleResult = await RoleManager.CreateAsync ( new IdentityRole ( role ) );
                }
            }
        }
    }
}
