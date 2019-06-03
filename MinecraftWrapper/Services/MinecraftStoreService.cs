using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationSettings _applicationSettings;

        public MinecraftStoreService ( ConsoleApplicationWrapper<MinecraftMessageParser> wrapper, StoreRepository storeRepository, UserRepository userRepository, 
            IServiceProvider serviceProvider, IOptions<ApplicationSettings> options )
        {
            _wrapper = wrapper;
            _storeRepository = storeRepository;
            _userRepository = userRepository;
            _serviceProvider = serviceProvider;
            _applicationSettings = options.Value;
        }

        public async Task AddCurrencyForUser ( string gamerTag, decimal amount, CurrencyTransactionReason currencyTransactionReason, CurrencyType currencyType = CurrencyType.Normal )
        {
            using ( var scope = _serviceProvider.CreateScope () )
            using ( var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>> () ) 
            {
                var user = userManager.Users.SingleOrDefault ( u => string.Equals ( gamerTag, u.GamerTag, StringComparison.CurrentCultureIgnoreCase ) );

                var uc = new UserCurrency
                {
                    Amount = amount,
                    CurrencyTransactionReasonId = currencyTransactionReason,
                    CurrencyTypeId = currencyType,
                    DateNoted = DateTime.UtcNow,
                    UserId = user.Id
                };

                await _storeRepository.SaveUserCurrency ( uc );
            }
        }

        public decimal GetDiscountedValueForUser (decimal value, ApplicationUser user)
        {
            var multiplier = user.Rank * _applicationSettings.DiscountPercentPerRank;
            multiplier = multiplier > _applicationSettings.DiscountRankCap ? _applicationSettings.DiscountRankCap : multiplier;
            return value * ( 1 - multiplier );
        }

        public async Task PurchaseItemAsync ( StoreItem item, ApplicationUser user )
        {
            var payment = new UserCurrency
            {
                Amount = GetDiscountedValueForUser(-item.Price, user) ,
                CurrencyTransactionReasonId = CurrencyTransactionReason.Purchase,
                CurrencyTypeId = CurrencyType.Normal,
                DateNoted = DateTime.UtcNow,
                UserId = user.Id,
                StoreItemId = item.StoreItemId
            };
            
            switch ( item.StoreItemTypeId )
            {
                case StoreItemType.Command:
                    _wrapper.SendInput ( ParseCommand ( item.Effect, user ), null );
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

            await _storeRepository.SaveUserCurrency ( payment );

            var gift = new UserCurrency
            {
                Amount = item.Price * _applicationSettings.GiftPointPercentage,
                CurrencyTransactionReasonId = CurrencyTransactionReason.Gift,
                CurrencyTypeId = CurrencyType.Gift,
                DateNoted = DateTime.UtcNow,
                UserId = user.Id,
                StoreItemId = null
            };

            await _storeRepository.SaveUserCurrency ( gift );
        }

        private string ParseCommand ( string effect, ApplicationUser user )
        {
            return effect.Replace ( "{GamerTag}", $"\"{user.GamerTag}\"", StringComparison.CurrentCultureIgnoreCase )
                         .Replace ( "{Rank}", user.Rank.ToString (), StringComparison.CurrentCultureIgnoreCase );
        }
    }
}
