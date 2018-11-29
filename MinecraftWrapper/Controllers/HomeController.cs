using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MinecraftWrapper.Models;
using MinecraftWrapper.Services;

namespace MinecraftWrapper.Controllers
{
    public class HomeController : Controller
    {
        private readonly ConsoleApplicationWrapper _wrapper;

        public HomeController (ConsoleApplicationWrapper wrapper)
        {
            _wrapper = wrapper;
        }

        public IActionResult Index ()
        {
            return View ();
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

        [HttpGet("[controller]/[action]")]
        public IEnumerable<string> FetchConsoleOutput ()
        {
            return _wrapper.StandardOutput;
        }
    }
}
