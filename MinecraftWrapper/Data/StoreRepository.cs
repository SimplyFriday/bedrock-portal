using Microsoft.EntityFrameworkCore;
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
                .ToListAsync ();
        }

        public async Task<IEnumerable<StoreItem>> GetAllItems ()
        {
            return await _context.StoreItem.ToListAsync ();
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

        public async Task<decimal> GetCurrencyTotalForUser ( string userId )
        {
            return await _context.UserCurrency
                .Where ( c => c.User.Id == userId )
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
    }
}
