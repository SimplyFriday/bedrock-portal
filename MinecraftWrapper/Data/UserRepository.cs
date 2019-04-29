using Microsoft.EntityFrameworkCore;
using MinecraftWrapper.Data.Constants;
using MinecraftWrapper.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftWrapper.Data
{
    public class UserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context )
        {
            _context = context;
        }

        public async Task<IList<ApplicationUser>> GetAllUsersAsync ()
        {
            return await _context.Users.ToListAsync ();
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

        public async Task<ApplicationUser> GetUserByDiscordIdAsync ( string discordId )
        {
            return await _context.Users.SingleOrDefaultAsync ( u => u.DiscordId == discordId );
            
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

        public async Task SaveUserAsync ( ApplicationUser user )
        {
            _context.Users.Update ( user );
            await _context.SaveChangesAsync ();
        }
        
        public async void InsertUserCurrencyAsync ( UserCurrency userCurrency )
        {
            if (userCurrency.UserCurrencyId != Guid.Empty )
            {
                throw new InvalidOperationException ( "UserCurrency objects can only be inserted, not updated." );
            }

            _context.UserCurrency.Add ( userCurrency );
            await _context.SaveChangesAsync ();
        }

        public async Task<ApplicationUser> GetUserByGamerTagAsync ( string gamerTag )
        {
            return await _context.Users.SingleOrDefaultAsync ( u => u.GamerTag == gamerTag );
        }

        public async Task PurchaseItem ( StoreItem item, ApplicationUser user )
        {
            var uc = new UserCurrency
            {
                Amount = -item.Price,
                CurrencyTransactionReasonId = CurrencyTransactionReason.Purchase,
                CurrencyTypeId = CurrencyType.Normal,
                User = user,
                DateNoted = DateTime.UtcNow
            };

            _context.UserCurrency.Add ( uc );
            await _context.SaveChangesAsync ();

            return;
        }
    }
}
