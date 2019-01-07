using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MinecraftWrapper.Data;
using MinecraftWrapper.Models;
using MinecraftWrapper.Services;
using Newtonsoft.Json;
using static MinecraftWrapper.Models.UpdateTickingAreaViewModel;

namespace MinecraftWrapper.Controllers
{
    [Authorize]
    public class UserActionsController : Controller
    {
        private readonly ConsoleApplicationWrapper<MinecraftMessageParser> _wrapper;
        private readonly UserRepository _userRepository;
        private readonly UserManager<AuthorizedUser> _userManager;
        private readonly WhiteListService _whiteListService;

        public UserActionsController ( ConsoleApplicationWrapper<MinecraftMessageParser> wrapper, UserRepository userRepository, UserManager<AuthorizedUser> userManager, WhiteListService whiteListService )
        {
            _wrapper = wrapper;
            _userRepository = userRepository;
            _userManager = userManager;
            _whiteListService = whiteListService;
        }

        [HttpGet]
        public async Task<IActionResult> UpdateTickingArea ()
        {
            var user = await _userManager.GetUserAsync ( HttpContext.User );
            var tickingAreas = GetTickingAreasByUser ( user );

            return View ( new UpdateTickingAreaViewModel { SavedTickingAreas = tickingAreas } );
        }

        private List<SavedTickingArea> GetTickingAreasByUser (IdentityUser user)
        {
            var preferences = _userRepository.GetUserPreferencesByUserId ( user.Id )
                                             .Where ( p => p.UserPreferenceType == UserPreferenceType.SavedTickingArea )
                                             .Select ( p => p.Value )
                                             .ToList ();

            var tickingAreas = new List<SavedTickingArea> ();

            foreach ( var p in preferences )
            {
                tickingAreas.Add ( JsonConvert.DeserializeObject<SavedTickingArea> ( p ) );
            }

            return tickingAreas;
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTickingArea ( UpdateTickingAreaViewModel model )
        {
            var user = await _userManager.GetUserAsync ( HttpContext.User );
            var data = _userRepository.GetAdditionalUserDataByUserId ( user.Id );
            
            if ( string.IsNullOrEmpty ( data?.GamerTag ) )
            {
                ModelState.AddModelError ( "", "An Xbox Live gamertag is required to use this function." );
            }
            else
            {
                _wrapper.SendInput ( $"tickingarea remove {data.GamerTag}" );
                _wrapper.SendInput ( $"tickingarea add circle {model.XCoord} 0 {model.ZCoord} 1 {data.GamerTag}" );

                if ( !string.IsNullOrEmpty ( model.Name ) )
                {
                    await _userRepository.SaveUserPreferance ( new UserPreference
                    {
                        UserId = user.Id,
                        UserPreferenceType = UserPreferenceType.SavedTickingArea,
                        Value = JsonConvert.SerializeObject ( new { model.XCoord, model.ZCoord, model.Name } )
                    } );
                }

                ViewBag.Status = "Update sent to server.";
            }

            var tickingAreas = GetTickingAreasByUser ( user );

            return View ( new UpdateTickingAreaViewModel { SavedTickingAreas = tickingAreas } );
        }
        
        [Authorize ( Roles = "Admin" )]
        [HttpGet]
        public IActionResult ManageWhiteList ()
        {
            var items = _whiteListService.GetWhiteListEntries ();

            return View (items);
        }

        [Authorize ( Roles = "Admin" )]
        [HttpGet]
        public IActionResult DeleteWhiteListEntry ( string name )
        {
            _whiteListService.DeleteWhiteListEntry ( name );
            return Redirect ( "ManageWhiteList" );
        }

        [Authorize ( Roles = "Admin" )]
        [HttpPost]
        public IActionResult AddWhiteListEntry ( string name )
        {
            _whiteListService.AddWhiteListEntry ( name );

            return Redirect ( "ManageWhiteList" );
        }

        [Authorize ( Roles = "Admin" )]
        [HttpGet]
        public IActionResult Console ()
        {
            return View ();
        }

        [Authorize ( Roles = "Admin" )]
        [HttpPost]
        public bool SendConsoleInput ([FromBody] string input)
        {
            if ( input != null )
            {
                _wrapper.SendInput ( input );
            }

            return true;
        }
    }
}