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

        public AuthorizationKey GetAuthorizationKeyByToken ( string authorizationToken )
        {
            return _context.AuthorizationKey.SingleOrDefault ( key => key.AuthorizationToken == authorizationToken );
        }

        public AuthorizationKey ReserveAuthorizationKey ( AuthorizationKey key, string userId )
        {
            key.UserId = userId;
            _context.SaveChanges ();
            return key;
        }

        public AdditionalUserData GetAdditionalUserDataByUserId ( string userId )
        {
            return _context.AdditionalUserData.SingleOrDefault ( data => data.UserId == userId );
        }

        public IQueryable<AuthorizedUser> GetUsersWithData ()
        {
            return _context.Users
                .Include ( u => u.AdditionalUserData )
                .Include ( u => u.AuthorizationKey );
        }

        public void SaveAdditionalData ( AdditionalUserData data )
        {
            if ( data.AdditionalUserDataId == Guid.Empty )
            {
                _context.AdditionalUserData.Add ( data );
            }
            else
            {
                _context.AdditionalUserData.Update ( data );
            }

            _context.SaveChanges ();
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
    }
}
