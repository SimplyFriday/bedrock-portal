﻿using Microsoft.AspNetCore.Identity;
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
        private readonly UserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public MinecraftStoreService ( ConsoleApplicationWrapper<MinecraftMessageParser> wrapper, StoreRepository storeRepository, UserRepository userRepository, UserManager<ApplicationUser> userManager )
        {
            _wrapper = wrapper;
            _storeRepository = storeRepository;
            _userRepository = userRepository;
            _userManager = userManager;
        }

        public async Task AddCurrencyForUser ( string gamerTag, int amount, CurrencyTransactionReason currencyTransactionReason )
        {
            var user = _userManager.Users.SingleOrDefault ( u => string.Equals ( gamerTag, u.GamerTag, StringComparison.CurrentCultureIgnoreCase ) );

            var uc = new UserCurrency
            {
                Amount = amount,
                CurrencyTransactionReasonId = currencyTransactionReason,
                CurrencyTypeId = CurrencyType.Normal,
                DateNoted = DateTime.UtcNow,
                UserId = user.Id
            };

            await _storeRepository.SaveUserCurrency ( uc );
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
            
            switch ( item.StoreItemTypeId )
            {
                case StoreItemType.Command:
                    _wrapper.SendInput ( item.Effect, null );
                    break;
                case StoreItemType.Membership:
                    int hours;

                    if ( int.TryParse ( item.Effect, out hours ) ) 
                    {
                        DateTime baseTime = ( user.MembershipExpirationTime ?? DateTime.UtcNow ) > DateTime.UtcNow ? user.MembershipExpirationTime.Value : DateTime.UtcNow;
                        user.MembershipExpirationTime = baseTime.AddHours ( hours );
                        await _userRepository.SaveUserAsync ( user );
                    }
                    else
                    {
                        throw new ArgumentException ( "The 'Effect' field must be an integer number of hours when using the 'Membership' StoreItemType" );
                    }
                    break;
                case StoreItemType.Rank:
                    int rankUp;

                    if ( int.TryParse ( item.Effect, out rankUp ) )
                    {
                        user.Rank += rankUp;
                        await _userRepository.SaveUserAsync ( user );
                    }
                    else
                    {
                        throw new ArgumentException ( "The 'Effect' field must be an integer number indicating how much Rank should increase by when using the 'Rank' StoreItemType" );
                    }
                    break;
                default:
                    throw new NotImplementedException ( $"You have tried using an unsupported StoreItemTypeId of {item.StoreItemTypeId}" );
            }

            await _storeRepository.SaveUserCurrency ( uc );
        }
    }
}