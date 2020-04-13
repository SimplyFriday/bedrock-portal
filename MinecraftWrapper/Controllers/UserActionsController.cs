﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MinecraftWrapper.Data;
using MinecraftWrapper.Data.Constants;
using MinecraftWrapper.Data.Entities;
using MinecraftWrapper.Models;
using MinecraftWrapper.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static MinecraftWrapper.Models.UpdateTickingAreaViewModel;

namespace MinecraftWrapper.Controllers
{
    [Authorize]
    public class UserActionsController : Controller
    {
        private readonly ConsoleApplicationWrapper<MinecraftMessageParser> _wrapper;
        private readonly UserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly WhiteListService _whiteListService;
        private readonly ApplicationSettings _applicationSettings;

        public UserActionsController ( ConsoleApplicationWrapper<MinecraftMessageParser> wrapper, UserRepository userRepository, UserManager<ApplicationUser> userManager, WhiteListService whiteListService, IOptions<ApplicationSettings> applicationSettings )
        {
            _wrapper = wrapper;
            _userRepository = userRepository;
            _userManager = userManager;
            _whiteListService = whiteListService;
            _applicationSettings = applicationSettings.Value;
        }

        [HttpGet]
        [Authorize(Roles = "NotAvailable")]
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
                                             .ToList ();

            var tickingAreas = new List<SavedTickingArea> ();

            foreach ( var p in preferences )
            {
                var ta = JsonConvert.DeserializeObject<SavedTickingArea> ( p.Value );
                ta.PreferenceId = p.UserPreferenceId;

                tickingAreas.Add ( ta );
            }

            return tickingAreas;
        }

        [HttpPost]
        [Authorize ( Roles = "NotAvailable" )]
        public async Task<IActionResult> UpdateTickingArea ( UpdateTickingAreaViewModel model )
        {
            var user = await _userManager.GetUserAsync ( HttpContext.User );
            
            if ( string.IsNullOrEmpty ( user.GamerTag ) )
            {
                ModelState.AddModelError ( "", SystemConstants.NO_GAMERTAG_ERROR );
            }
            else
            {
                _wrapper.SendInput ( $"tickingarea remove {user.GamerTag}", null );
                _wrapper.SendInput ( $"tickingarea add circle {model.XCoord} 0 {model.ZCoord} 1 {user.GamerTag}", null );

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
        
        public async Task<IActionResult> ClearMobs ()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var lastRequest = _userRepository.GetLastUtilityRequestByType ( UtilityRequestType.ClearMobs, user.Id );

            return View ( lastRequest?.RequestTime );
        }

        [HttpPost]
        [Authorize ( Roles = "NotAvailable" )]
        public async Task<IActionResult> ClearMobs (string needDifferentSignature)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var lastRequest = _userRepository.GetLastUtilityRequestByType ( UtilityRequestType.ClearMobs, user.Id );
            DateTime? lastUsed = lastRequest?.RequestTime;

            if ( lastUsed == null || lastUsed.Value.AddSeconds(SystemConstants.CLEAR_MOBS_COOLDOWN) < DateTime.UtcNow )
            {
                if ( user.GamerTag != null )
                {
                    var newRequest = new UtilityRequest { RequestTime = DateTime.UtcNow, UserId = user.Id, UtilityRequestType = UtilityRequestType.ClearMobs };
                    _userRepository.SaveUtilityRequestAsync ( newRequest );
                    lastUsed = DateTime.UtcNow;

                    foreach ( var mob in _applicationSettings.MobsToClear )
                    {
                        var command = $"execute {user.GamerTag} ~ ~ ~ kill @e[r=65, type={mob}, name=!KEEPME]";
                        _wrapper.SendInput ( command, null );

                        // These guys area special because they need to be killed several times, and it takes some time before the smaller ones spawn.
                        if (mob == "slime" || mob == "magma_cube" )
                        {
                            Thread.Sleep ( 1000 );
                        }
                    }

                    ViewBag.Status = "Request processed.";
                }
                else
                {
                    if ( string.IsNullOrEmpty ( user.GamerTag ) )
                    {
                        ViewBag.Status = SystemConstants.NO_GAMERTAG_ERROR;
                    }
                }
            }

            return View ( lastUsed );
        }

        [Authorize ( Roles = "Admin,Moderator" )]
        [HttpGet]
        public IActionResult ManageWhiteList ()
        {
            var items = _whiteListService.GetWhiteListEntries ();

            return View (items);
        }

        [Authorize ( Roles = "Admin,Moderator" )]
        [HttpGet]
        public IActionResult DeleteWhiteListEntry ( string name )
        {
            _whiteListService.DeleteWhiteListEntry ( name );
            return Redirect ( "ManageWhiteList" );
        }

        [Authorize ( Roles = "Admin,Moderator" )]
        [HttpPost]
        public IActionResult AddWhiteListEntry ( string name )
        {
            _whiteListService.AddWhiteListEntry ( name );

            return Redirect ( "ManageWhiteList" );
        }

        [Authorize ( Roles = "Admin,Moderator" )]
        [HttpGet]
        public IActionResult Console ()
        {
            return View ();
        }

        [HttpDelete]
        [Authorize ( Roles = "NotAvailable" )]
        public async Task<bool> DeleteSavedTickingArea ([FromQuery] string name )
        {
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                var tickingAreas = GetTickingAreasByUser ( user );

                var itemsToRemove = tickingAreas
                                    .Where(ta => ta.Name == name)
                                    .Select(ta=>ta.PreferenceId);

                _userRepository.DeleteUserPreferencesByIdAsync ( itemsToRemove );
            } catch
            {
                // TODO log something, jackass
                return false;
            }

            return true;
        }

        [ Authorize ( Roles = "Admin,Moderator" )]
        [HttpPost]
        public async Task<bool> SendConsoleInput ([FromBody] string input)
        {
            if ( input != null )
            {
                var user = await _userManager.GetUserAsync ( HttpContext.User );

                var commands = await GetCommandsForUser ( user );
                var canRun = false;
                var sInput = input.ToLower();

                foreach ( var command in commands )
                {
                    if ( sInput.StartsWith ( command ) || commands.Contains ( "*" ) )  
                    {
                        canRun = true;
                        break;
                    }
                }

                if ( canRun )
                {
                    _wrapper.SendInput ( input, user.Id );
                }
                else
                {
                    _wrapper.AddEphemeralMessage ( $"You do not have permission to run the command '{input}'", user.Id );
                }
                
            }

            return true;
        }

        [HttpGet]
        [Authorize(Roles ="Admin,Moderator")]
        public async Task<IActionResult> ManageUsers ()
        {
            var users = await _userRepository.GetAllUsersAsync();
            var model = new ManageUsersViewModel ();

            foreach (var user in users )
            {
                var item = new ManageUsersViewModel.ManageUserItem
                {
                    UserId = user.Id,
                    IsAdmin = await _userManager.IsInRoleAsync(user, "Admin"),
                    IsModerator = await _userManager.IsInRoleAsync (user,"Moderator"),
                    CurrentMoney = user.CurrentMoney,
                    DiscordId = user.DiscordId,
                    GamerTag = user.GamerTag,
                    IsActive = user.IsActive,
                    MembershipExpirationTime = user.MembershipExpirationTime,
                    Rank = user.Rank
                };

                model.Users.Add ( item );
            }

            return View ( model );
        }

        [HttpGet]
        [Authorize ( Roles = "Admin,Moderator" )]
        public async Task<IActionResult> ToggleUserActiveState (string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null )
            {
                return NotFound ();
            }

            user.IsActive = !user.IsActive;
            await _userRepository.SaveUserAsync ( user );

            return RedirectToAction ( "ManageUsers" );
        }

        [HttpGet]
        [Authorize ( Roles = "Admin,Moderator" )]
        public async Task<IActionResult> ToggleUserActive ( [FromBody] string userId )
        {
            var user = await _userManager.FindByIdAsync(userId);

            if ( user != null )
            {
                user.IsActive = !user.IsActive;
                await _userRepository.SaveUserAsync ( user );
            }

            return RedirectToAction ( "ManageUsers" );
        }

        [HttpGet]
        [Authorize ( Roles = "Admin" )]
        public async Task<IActionResult> ToggleAdmin (string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if ( user == null )
            {
                return NotFound ();
            }

            var wasInRole = await _userManager.IsInRoleAsync (user, "Admin");

            var remainingAdmins = await _userManager.GetUsersInRoleAsync ( "Admin" );

            // Can't remove last admin... safety first...
            if ( remainingAdmins.Count == 1 && remainingAdmins.Single ().Id == id )
            {
                return RedirectToAction ( "ManageUsers" );
            }

            await _userManager.RemoveFromRolesAsync ( user, new string[] { "Admin", "Moderator" } );

            if ( !wasInRole ) 
            {
                await _userManager.AddToRoleAsync ( user, "Admin" );
            }

            await _userManager.UpdateAsync ( user );

            return RedirectToAction ( "ManageUsers" );
        }

        [HttpGet]
        [Authorize ( Roles = "Admin" )]
        public async Task<IActionResult> ToggleModerator ( string id )
        {
            var user = await _userManager.FindByIdAsync(id);

            if ( user == null )
            {
                return NotFound ();
            }

            var wasInRole = await _userManager.IsInRoleAsync (user, "Moderator");
            await _userManager.RemoveFromRolesAsync ( user, new string[] { "Admin", "Moderator" } );

            if ( !wasInRole )
            {
                await _userManager.AddToRoleAsync ( user, "Moderator" );
            }

            return RedirectToAction ( "ManageUsers" );
        }

        private async Task<IEnumerable<string>> GetCommandsForUser ( ApplicationUser user )
        {
            var roles = await _userManager.GetRolesAsync ( user );

            var commands = new List<string>();

            // Short circuit for amin
            if ( roles.Any ( r => r.ToLower () == "admin" ) )
            {
                commands.Add ( "*" );
                return commands;
            }

            foreach ( var crw in _applicationSettings.CommandWhitelistByRole )
            {
                if (roles.Any(r=>r.ToLower() == crw.RoleName.ToLower () ) )
                {
                    commands.AddRange ( crw.Commands );
                }
            }

            return commands;
        }
    }
}