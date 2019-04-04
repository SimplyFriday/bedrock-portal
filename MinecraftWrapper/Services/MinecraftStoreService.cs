using Microsoft.AspNetCore.Identity;
using MinecraftWrapper.Data;
using MinecraftWrapper.Data.Constants;
using MinecraftWrapper.Data.Entities;
using MinecraftWrapper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftWrapper.Services
{
    public class MinecraftStoreService
    {
        private readonly ConsoleApplicationWrapper<MinecraftMessageParser> _wrapper;
        private readonly StoreRepository _storeRepository;

        public MinecraftStoreService ( ConsoleApplicationWrapper<MinecraftMessageParser> wrapper, StoreRepository storeRepository )
        {
            _wrapper = wrapper;
            _storeRepository = storeRepository;
        }

        public int GetMinecraftCurrencyForUser ( string gamerTag )
        {
            throw new NotImplementedException();
        }

        public void AddCurrencyForUser ( string gamerTag, int amount )
        {
            throw new NotImplementedException ();
        }

        public async Task PurchaseItemAsync ( StoreItem item, ApplicationUser user )
        {
            var uc = new UserCurrency
            {
                Amount = -item.Price,
                CurrencyTransactionReasonId = CurrencyTransactionReason.Purchase,
                CurrencyTypeId = CurrencyType.Normal,
                DateNoted = DateTime.UtcNow,
                UserId = user.Id
            };

            await _storeRepository.SaveUserCurrency ( uc );

            switch ( item.StoreItemTypeId )
            {
                case StoreItemType.Command:
                    _wrapper.SendInput ( item.Effect, null );
                    break;
                default:
                    throw new NotImplementedException ( $"You have tried using an unsupported StoreItemTypeId of {item.StoreItemTypeId}" );
            }
        }
    }
}
