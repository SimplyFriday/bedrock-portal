using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MinecraftWrapper.Services
{
    public class MinecraftMessageParser : IMessageParser
    {
        private const string PLAYER_CONNECTED = "Player connected: ";
        private const string PLAYER_DISCONNECTED = "Player disconnected: ";

        private readonly StatusService _statusService;

        public MinecraftMessageParser (StatusService statusService)
        {
            _statusService = statusService;
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

        private void ChangePlayerOnlineStatus(string needle, string haystack, bool status )
        {
            Regex regex = new Regex ( $"{needle}(\\d+)" );
            var matches = regex.Matches ( haystack );

            if ( matches.Count () > 0 )
            {
                var id = matches[ 0 ].Groups[ 1 ].Value;
                _statusService.UpdateUserStatus ( id, status );
            }
        }
    }
}
