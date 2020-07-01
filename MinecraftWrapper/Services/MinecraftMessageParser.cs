using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MinecraftWrapper.Data;
using MinecraftWrapper.Data.Constants;
using MinecraftWrapper.Data.Entities;
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
        private const string SERVER_STOPPED = "Server stop requested";

        private readonly StatusService _statusService;
        private readonly WhiteListService _whiteListService;
        private readonly DiscordService _discordService;
        private readonly ApplicationSettings _applicationSettings;
        private readonly IServiceProvider _serviceProvider;

        private List<ScheduledTask> _playerHooksCache;
        private DateTime _hookLastRefresh;

        public MinecraftMessageParser ( StatusService statusService, WhiteListService whiteListService, DiscordService discordService, IOptions<ApplicationSettings> options, IServiceProvider serviceProvider)
        {
            _statusService = statusService;
            _whiteListService = whiteListService;
            _discordService = discordService;
            _applicationSettings = options.Value;
            _serviceProvider = serviceProvider;
        }

        public bool FilterInput ( string input )
        {
            // This application does not care about input
            return true;
        }

        public async void HandleOutput ( string output )
        {
            if ( output.Contains ( SERVER_STOPPED ) )
            {
                await LogAllPlayersOut ();
            }

            if ( _playerHooksCache == null ||
                 _hookLastRefresh < DateTime.UtcNow.AddSeconds (-30) )
            {
                _hookLastRefresh = DateTime.UtcNow;

                using ( var scope = _serviceProvider.CreateScope () )
                {
                    _playerHooksCache = await scope.ServiceProvider
                        .GetRequiredService<ScheduledTaskRepository> ()
                        .GetAllPlayerHooksAsync ();
                }
            }

            await CheckForPlayerOnlineStatusChange ( PLAYER_CONNECTED, output, true );
            await CheckForPlayerOnlineStatusChange ( PLAYER_DISCONNECTED, output, false );            
        }

        private async Task LogAllPlayersOut ()
        {
            try
            {
                using ( var scope = _serviceProvider.CreateScope () )
                {
                    var statusService = scope.ServiceProvider.GetRequiredService<StatusService> ();

                    foreach ( var player in statusService.GetAllUsersByState ( true ) )
                    {
                        await ChangePlayerStatus ( player, false );
                    }
                }
            }
            catch ( Exception ex )
            {
                Log.Error ( ex, $"An error occurred in {nameof ( LogAllPlayersOut )}()" );
            }
        }

        /// <summary>
        /// This method updates the player's online state based on the target status and the message coming in. It also sends out a discord webhook notification, if present.
        /// </summary>
        /// <param name="needle">Phrase to search for</param>
        /// <param name="haystack">Text to search</param>
        /// <param name="isOnline">What to change the player's online state to - true for logged in, false for logged out</param>
        /// <returns>Returns true if the needle was found and the status was successfully updated</returns>
        private async Task<bool> CheckForPlayerOnlineStatusChange (string needle, string haystack, bool isOnline )
        {
            Regex regex = new Regex ( $"{needle}(.+?),", RegexOptions.IgnoreCase);
            var matches = regex.Matches ( haystack );
            
            if ( matches.Count () > 0 )
            {
                try
                {
                    var gamertag = matches[ 0 ].Groups[ 1 ].Value;

                    if ( gamertag != null )
                    {
                        await ChangePlayerStatus ( gamertag, isOnline );

                        var type = isOnline ? ScheduledTaskType.PlayerLogin : ScheduledTaskType.PlayerLogout;

                        using ( var scope = _serviceProvider.CreateScope () )
                        {
                            var wrapper = scope.ServiceProvider.GetRequiredService<ConsoleApplicationWrapper<MinecraftMessageParser>> ();

                            foreach ( var task in _playerHooksCache.Where ( t => t.ScheduledTaskType == type ) )
                            {
                                var commands = task.Command.Split ( '\n' );

                                foreach ( var command in commands )
                                {
                                    wrapper.SendInput ( command.Replace ( "{gamertag}", gamertag ), null );
                                }
                            }
                        }
                    }

                    return true;
                } catch (Exception ex)
                {
                    Log.Error ( ex, $"An error occurred in {nameof ( CheckForPlayerOnlineStatusChange )}({needle},{haystack},{isOnline})" );
                }
            }

            return false;
        }

        private async Task ChangePlayerStatus ( string gamertag, bool isOnline )
        {
            using ( var scope = _serviceProvider.CreateScope () )
            {
                var userRepository = scope.ServiceProvider.GetRequiredService<UserRepository>();
                var minecraftStoreService = scope.ServiceProvider.GetRequiredService<MinecraftStoreService>();

                _statusService.UpdateUserStatus ( gamertag, isOnline );
                var user = await userRepository.GetUserByGamerTagAsync ( gamertag );

                if ( isOnline )
                {
                    // Player has logged in
                    if ( user != null && ( user.LastLoginReward == null || user.LastLoginReward.Value.AddDays ( 1 ) <= DateTime.UtcNow ) )
                    {
                        // daily login bonus
                        await minecraftStoreService.AddCurrencyForUser ( gamertag, _applicationSettings.DailyLoginBonus, CurrencyTransactionReason.DailyLogin );
                        user.LastLoginReward = DateTime.UtcNow;
                    }

                    user.LastMinecraftLogin = DateTime.UtcNow;
                    await userRepository.SaveUserAsync ( user );
                    await userRepository.AddPlaytimeEvent ( user, PlaytimeEvent.LOGIN_EVENT_CODE );

                    _discordService.SendWebhookMessage ( $"{gamertag} has logged in!" );
                }
                else
                {
                    // Player has logged out
                    if ( user.LastMinecraftLogin.HasValue )
                    {
                        var seconds = (DateTime.UtcNow - user.LastMinecraftLogin.Value).TotalSeconds;

                        if ( _applicationSettings.StoreEnabled )
                        {
                            await minecraftStoreService.AddCurrencyForUser ( gamertag, (decimal) seconds * _applicationSettings.PointsPerSecond, CurrencyTransactionReason.TimePlayed );
                        }

                        await userRepository.AddPlaytimeEvent ( user, PlaytimeEvent.LOGOUT_EVENT_CODE );
                    }
                }
            }
        }
    }
}
