using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MinecraftWrapper.Data;
using MinecraftWrapper.Data.Constants;
using MinecraftWrapper.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MinecraftWrapper.Services
{
    public class MinecraftMessageParser : IMessageParser
    {
        private const string PLAYER_CONNECTED = "player connected: ";
        private const string PLAYER_DISCONNECTED = "player disconnected: ";

        private readonly StatusService _statusService;
        private readonly WhiteListService _whiteListService;
        private readonly DiscordService _discordService;
        private readonly UserRepository _userRepository;
        private readonly MinecraftStoreService _minecraftStoreService;
        private readonly ApplicationSettings _applicationSettings;

        public MinecraftMessageParser ( StatusService statusService, WhiteListService whiteListService, DiscordService discordService, UserRepository userRepository,
            MinecraftStoreService minecraftStoreService, IOptions<ApplicationSettings> options)
        {
            _statusService = statusService;
            _whiteListService = whiteListService;
            _discordService = discordService;
            _userRepository = userRepository;
            _minecraftStoreService = minecraftStoreService;
            _applicationSettings = options.Value;
        }

        public bool FilterInput ( string input )
        {
            // This application does not care about input
            return true;
        }

        public async void HandleOutput ( string output )
        {
            await ChangePlayerOnlineStatus ( PLAYER_CONNECTED, output, true );
            await ChangePlayerOnlineStatus ( PLAYER_DISCONNECTED, output, false );
        }

        /// <summary>
        /// This method updates the player's online state based on the target status and the message coming in. It also sends out a discord webhook notification, if present.
        /// </summary>
        /// <param name="needle">Phrase to search for</param>
        /// <param name="haystack">Text to search</param>
        /// <param name="status">What to change the player's online state to - true for logged in, false for logged out</param>
        /// <returns>Returns true if the needle was found and the status was successfully updated</returns>
        private async Task<bool> ChangePlayerOnlineStatus(string needle, string haystack, bool status )
        {
            Regex regex = new Regex ( $"{needle}(.+?),", RegexOptions.IgnoreCase);
            var matches = regex.Matches ( haystack );
            
            if ( matches.Count () > 0 )
            {
                try
                {
                    var gamerTag = matches[ 0 ].Groups[ 1 ].Value;

                    if ( gamerTag != null )
                    {
                        _statusService.UpdateUserStatus ( gamerTag, status );
                        var user = await _userRepository.GetUserByGamerTagAsync ( gamerTag );

                        if ( status )
                        {
                            // Player has logged in
                            if ( user != null && (user.LastLoginReward == null || user.LastLoginReward.Value.AddDays ( 1 ) <= DateTime.UtcNow) )
                            {
                                // daily login bonus
                                await _minecraftStoreService.AddCurrencyForUser ( gamerTag, _applicationSettings.DailyLoginBonus, CurrencyTransactionReason.DailyLogin );
                                user.LastLoginReward = DateTime.UtcNow;
                            }

                            user.LastMinecraftLogin = DateTime.UtcNow;
                            await _userRepository.SaveUserAsync ( user );

                            _discordService.SendWebhookMessage ( $"{gamerTag} has logged in!" );
                        }
                        else
                        {
                            // Player has logged out
                            if ( user.LastMinecraftLogin.HasValue )
                            {
                                var seconds = (DateTime.UtcNow - user.LastMinecraftLogin.Value).TotalSeconds;
                                await _minecraftStoreService.AddCurrencyForUser ( gamerTag, (decimal) seconds * _applicationSettings.PointsPerSecond, CurrencyTransactionReason.TimePlayed );
                            }
                        }
                    }

                    return true;
                } catch (Exception ex)
                {
                    Log.Error ( ex, $"An error occurred in ChangePlayerOnlineStatus({needle},{haystack},{status})" );
                }
            }

            return false;
        }
    }
}
