using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace MinecraftWrapper.Controllers
{
    /// <summary>
    /// This controller is meant to be used by the dev team to force scenarios. Wrap everything in this to prevent use in prod
    ///     if ( _hostingEnvironment.IsDevelopment () ) { DO STUFF HERE }
    /// </summary>
    public class DebugController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public DebugController(IHostingEnvironment env )
        {
            _hostingEnvironment = env;
        }

        public IActionResult Crash()
        {
            if ( _hostingEnvironment.IsDevelopment () )
            {
                throw new Exception ( "This is a crash test." );
            }

            return RedirectToAction ( "Index", "Home" );
        }
    }
}