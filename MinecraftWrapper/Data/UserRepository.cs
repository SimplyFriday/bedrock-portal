using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MinecraftWrapper.Data.Constants;
using MinecraftWrapper.Models;

namespace MinecraftWrapper.Data
{
    public class UserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context )
        {
            _context = context;
        }

        public IQueryable<ApplicationUser> GetAllUsers ()
        {
            return _context.Users;
        }

        public async Task<bool> SaveUserPreferance ( UserPreference userPreference )
        {
            if ( userPreference.UserPreferenceId == Guid.Empty )
            {
                _context.UserPreference.Add ( userPreference );
            }
            else
            {
                _context.UserPreference.Update ( userPreference );
            }

            await _context.SaveChangesAsync ();

            return true;
        }

        public IQueryable<UserPreference> GetUserPreferencesByUserId ( string userId )
        {
            return _context.UserPreference.Where ( p => p.User.Id == userId );
        }

        public UtilityRequest GetLastUtilityRequestByType ( UtilityRequestType utilityRequestType, string userId )
        {
            return _context.UtilityRequest
                .OrderByDescending(ur => ur.RequestTime)
                .FirstOrDefault ( ur => ur.UtilityRequestType == utilityRequestType  && ur.UserId == userId );
        }

        public async void SaveUtilityRequestAsync ( UtilityRequest newRequest )
        {
            _context.UtilityRequest.Add ( newRequest );
            await _context.SaveChangesAsync ();
        }

        public async void DeleteUserPreferencesByIdAsync ( IEnumerable<Guid> ids )
        {
            foreach ( var id in ids ) 
            {
                var preference = new UserPreference {UserPreferenceId = id};

                _context.UserPreference.Attach ( preference );
                _context.UserPreference.Remove ( preference );
            }

            await _context.SaveChangesAsync ();
        }

        public async void SaveUserAsync ( ApplicationUser user )
        {
            _context.Users.Update ( user );
            await _context.SaveChangesAsync ();
        }
    }
}
