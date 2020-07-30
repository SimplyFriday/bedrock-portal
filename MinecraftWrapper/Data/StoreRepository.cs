using Microsoft.EntityFrameworkCore;
using MinecraftWrapper.Data.Constants;
using MinecraftWrapper.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftWrapper.Data
{
    public class StoreRepository
    {
        private readonly ApplicationDbContext _context;

        public StoreRepository ( ApplicationDbContext context )
        {
            _context = context;
        }

        public async Task<IEnumerable<StoreItem>> GetAvailableStoreItemsByRank ( int rank )
        {
            return await _context.StoreItem
                .Where ( s => s.MinimumRank <= rank )
                .AsNoTracking ()
                .ToListAsync ();
        }

        public async Task<IEnumerable<StoreItem>> GetAllItems ()
        {
            return await _context.StoreItem
                .AsNoTracking ()
                .ToListAsync ();
        }

        public async Task<StoreItem> GetStoreItemByIdAsync ( Guid? id )
        {
            return await _context.StoreItem
                .FirstOrDefaultAsync ( m => m.StoreItemId == id );
        }

        public async Task SaveStoreItemAsync ( StoreItem storeItem )
        {
            if ( storeItem.StoreItemId == Guid.Empty )
            {
                _context.Add ( storeItem );
            }
            else
            {
                _context.Update ( storeItem );
            }

            await _context.SaveChangesAsync ();
        }

        public bool StoreItemExists ( Guid id )
        {
            return _context.StoreItem.Any ( e => e.StoreItemId == id );
        }

        public async Task SaveUserCurrency ( UserCurrency userCurrency )
        {
            if ( userCurrency.UserCurrencyId == Guid.Empty )
            {
                _context.Add ( userCurrency );
            }
            else
            {
                _context.Update ( userCurrency );
            }

            await _context.SaveChangesAsync ();
        }

        public async Task<decimal> GetCurrencyTotalForUserAsync ( string userId, CurrencyType currencyType )
        {
            return await _context.UserCurrency
                .Where ( c => c.User.Id == userId && c.CurrencyTypeId == currencyType )
                .Select ( c => c.Amount )
                .SumAsync ();
        }

        public async Task DeleteStoreItemAsync ( StoreItem item )
        {
            if (item != null )
            {
                _context.StoreItem.Remove ( item );
            }

            await _context.SaveChangesAsync ();
        }

        public UserCurrency GetMostRecentUserCurrencyByUserIdAndReason ( string id, CurrencyTransactionReason currencyTransactionReason )
        {
            return _context.UserCurrency
                .OrderBy ( uc => uc.DateNoted )
                .LastOrDefault ( uc => uc.UserId == id && uc.CurrencyTransactionReasonId == currencyTransactionReason );
        }

        public async Task<List<UserCurrency>> GetUserGiftsSentByUserIdAsyc ( string id )
        {
            return await _context.UserCurrency
                            .Where ( uc =>  uc.UserId == id && 
                                            uc.CurrencyTypeId == CurrencyType.Gift &&
                                            uc.CurrencyTransactionReasonId == CurrencyTransactionReason.Gift )
                            .ToListAsync ();
        }

        public async Task<string> GetGamertagFromSentGiftAsyc ( Guid id )
        {
            return await _context.UserCurrency
                            .Where ( uc => uc.UserCurrencyId == id )
                            .Select ( uc => uc.User.GamerTag )
                            .SingleOrDefaultAsync ();
        }

        public async Task<List<UserCurrency>> GetUserReceivedGiftCurrenciesByUserIdAsyc ( string id )
        {
            return await _context.UserCurrency
                            .Where ( uc => uc.UserId == id
                                            && uc.CurrencyTypeId == CurrencyType.Normal
                                            && uc.CurrencyTransactionReasonId == CurrencyTransactionReason.Gift )
                                .Include ( uc => uc.CreatedFromTransaction )
                                    .ThenInclude ( uc => uc.User )
                            .ToListAsync ();
        }


    }
}
