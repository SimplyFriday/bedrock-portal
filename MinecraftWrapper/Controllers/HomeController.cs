using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MinecraftWrapper.Data;
using MinecraftWrapper.Models;
using MinecraftWrapper.Services;

namespace MinecraftWrapper.Controllers
{
    public class HomeController : Controller
    {
        private readonly ConsoleApplicationWrapper _wrapper;
        private readonly UserRepository _userRepository;

        public HomeController ( ConsoleApplicationWrapper wrapper, UserRepository userRepository )
        {
            _wrapper = wrapper;
            _userRepository = userRepository;
        }

        public IActionResult Index ()
        {
            var model = new HomeIndexViewModel
            {
                Users = _userRepository.GetUsersWithData ()
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
            return View ( new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier } );
        }

        [Authorize]
        [HttpGet("[controller]/[action]")]
        public IEnumerable<string> FetchConsoleOutput ()
        {
            return _wrapper.StandardOutput;
        }

        [HttpGet]
        public IActionResult CommunityGuidelines()
        {
            return View ();
        }
    }
}
