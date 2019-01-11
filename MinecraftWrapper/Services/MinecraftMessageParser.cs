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

        public MinecraftMessageParser ( StatusService statusService, WhiteListService whiteListService, DiscordService discordService )
        {
            _statusService = statusService;
            _whiteListService = whiteListService;
            _discordService = discordService;
        }

        public bool FilterInput ( string input )
        {
            // This application does not care about input
            return true;
        }

        public void HandleOutput ( string output )
        {
            ChangePlayerOnlineStatus ( PLAYER_CONNECTED, output, true );
            ChangePlayerOnlineStatus ( PLAYER_DISCONNECTED, output, false );
        }

        /// <summary>
        /// This method updates the player's online state based on the target status and the message coming in. It also sends out a discord webhook notification, if present.
        /// </summary>
        /// <param name="needle">Phrase to search for</param>
        /// <param name="haystack">Text to search</param>
        /// <param name="status">What to change the player's online state to - true for logged in, false for logged out</param>
        /// <returns>Returns true if the needle was found and the status was successfully updated</returns>
        private bool ChangePlayerOnlineStatus(string needle, string haystack, bool status )
        {
            needle = needle.ToLower ();
            haystack = haystack.ToLower ();

            Regex regex = new Regex ( $"{needle}(\\d+)" );
            var matches = regex.Matches ( haystack );

            if ( matches.Count () > 0 )
            {
                var id = matches[ 0 ].Groups[ 1 ].Value;
                _statusService.UpdateUserStatus ( id, status );

                var player = _whiteListService.GetWhiteListEntries ().SingleOrDefault ( w => w.xuid == id );

                if ( status && player != null )
                {
                    // Player has logged in, notify Discord
                    _discordService.SendWebhookMessage ( $"{player.name} has logged in!" );
                }

                return true;
            }

            return false;
        }
    }
}
