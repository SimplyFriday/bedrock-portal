using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MinecraftWrapper.Data;
using MinecraftWrapper.Models;
using MinecraftWrapper.Services;
using Serilog;

namespace MinecraftWrapper.Controllers
{
    public class HomeController : Controller
    {
        private readonly ConsoleApplicationWrapper<MinecraftMessageParser> _wrapper;
        private readonly UserRepository _userRepository;
        private readonly SystemRepository _systemRepository;
        private readonly IHostingEnvironment _hostingEnvironment;

        public HomeController ( ConsoleApplicationWrapper<MinecraftMessageParser> wrapper, UserRepository userRepository, SystemRepository systemRepository, IHostingEnvironment env )
        {
            _wrapper = wrapper;
            _userRepository = userRepository;
            _systemRepository = systemRepository;
            _hostingEnvironment = env;
        }

        public IActionResult Index ()
        {
            var model = new HomeIndexViewModel
            {
                Users = _userRepository.GetAllUsers (),
                NewsItems = _systemRepository.GetRecentNewsItems ( 5 ),
            };

            return View (model);
        }

        public IActionResult About ()
        {
            ViewData[ "Message" ] = "Your application description page.";

            return View ();
        }

        public IActionResult Contact ()
        {
            ViewData[ "Message" ] = "Your contact page.";

            return View ();
        }

        public IActionResult Privacy ()
        {
            return View ();
        }

        [ResponseCache ( Duration = 0, Location = ResponseCacheLocation.None, NoStore = true )]
        public IActionResult Error ()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var viewModel = new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };

            if ( exceptionFeature != null )
            {
                if ( _hostingEnvironment.IsDevelopment () )
                {
                    viewModel.Exception = exceptionFeature.Error;
                }

                try
                {
                    // TODO break out TraceIdentifier into its own column so that we can index it.
                    Log.Error ( exceptionFeature.Error, "Unhandled exception occurred from URL {Url} with {TraceIdentifier}", exceptionFeature.Path, HttpContext.TraceIdentifier );
                }
                catch
                {
                    // Nothing left to do here, I guess :(
                }
            }

            return View ( viewModel );
        }

        [Authorize ( Roles = "Admin,Moderator" )]
        [HttpGet("[controller]/[action]")]
        public IEnumerable<string> FetchConsoleOutput ()
        {
            return _wrapper.StandardOutput;
        }

        [HttpGet( "[controller]/[action]/{pageName}" )]
        public IActionResult Static ( string pageName )
        {
            var path = $"{_hostingEnvironment.ContentRootPath}/wwwroot/html_frag/{pageName}.html";
            var model = new StaticPageViewModel { Title = "Content Not Found" };

            if (System.IO.File.Exists( path ) )
            {
                model.Content = System.IO.File.ReadAllText ( path );
                model.Title = pageName;
            }

            return View ( model );
        }
    }
}
