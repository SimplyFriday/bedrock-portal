using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MinecraftWrapper.Data;
using MinecraftWrapper.Data.Entities;
using MinecraftWrapper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftWrapper.Controllers
{
    [Authorize(Roles="Admin, Moderator")]
    public class UsersController : Controller
    {
        private readonly UserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController ( UserRepository userRepository, UserManager<ApplicationUser> userManager )
        {
            _userRepository = userRepository;
            _userManager = userManager;
        }

        public async Task<IActionResult> ManageUserDetails (string id)
        {
            var model = new ManageUserDetailsViewModel
            {
                UserRole = 0,
                SearchCutoff = DateTime.UtcNow.AddDays ( -21 )
            };

            try
            {
                await PopulateViewModel ( model, id );
            } catch ( KeyNotFoundException )
            {
                return NotFound ();
            }

            return View ( model );
        }

        [HttpPost]
        public async Task<IActionResult> ManageUserDetails ( [FromForm] ManageUserDetailsViewModel model )
        {
            var newRole = model.UserRole;
            var newIsActive = model.User.IsActive;

            try
            {
                await PopulateViewModel ( model, model.User.Id );
            }
            catch ( KeyNotFoundException )
            {
                return NotFound ();
            }

            if ( newRole != model.UserRole )
            {
                var newRoleString = model.UserRoles[newRole].Text;
                var oldRoleString = model.UserRoles[model.UserRole].Text;

                var wasInRole = await _userManager.IsInRoleAsync (model.User, newRoleString);

                // Can't remove last admin... safety first...
                if ( oldRoleString == "Admin" )
                {
                    var remainingAdmins = await _userManager.GetUsersInRoleAsync ( "Admin" );
                    
                    if ( remainingAdmins.Count == 1 && remainingAdmins.Single ().Id == model.User.Id )
                    {
                        ViewBag.Status = "ERROR: Cannot remove the last admin!";
                        return View ( model );
                    }
                }

                await _userManager.RemoveFromRolesAsync ( model.User, new string[] { "Admin", "Moderator" } );

                if ( !wasInRole && newRole > 0 )
                {
                    await _userManager.AddToRoleAsync ( model.User, newRoleString );
                }
            }

            model.User.IsActive = newIsActive;
            await _userManager.UpdateAsync ( model.User );

            model.UserRole = newRole;

            ViewBag.Status = "User Updated!";
            return View ( model );
        }

        private async Task PopulateViewModel ( ManageUserDetailsViewModel model, string id )
        {
            var user = await _userManager.FindByIdAsync ( id );

            if ( user == null )
            {
                throw new KeyNotFoundException ();
            }
            else
            {
                model.User = user;
            }

            model.RecentEvents = await _userRepository.GetPlaytimeEventsByUserIDSinceDateAsync ( model.User.Id, model.SearchCutoff );

            var isModerator = await _userManager.IsInRoleAsync ( model.User, "Moderator" );
            var isAdmin = await _userManager.IsInRoleAsync ( model.User, "Admin" );

            model.UserRole = 0;

            if ( isModerator )
            {
                model.UserRole = 1;
            }
            else if ( isAdmin )
            {
                model.UserRole = 2;
            }
        }
    }
}