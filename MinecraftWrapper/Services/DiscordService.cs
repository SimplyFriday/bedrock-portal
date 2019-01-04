using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MinecraftWrapper.Data;
using Newtonsoft.Json;

namespace MinecraftWrapper.Services
{
    public class DiscordService
    {
        private readonly ApplicationSettings _applicationSettings;

        public DiscordService ( IOptions<ApplicationSettings> options )
        {
            _applicationSettings = options.Value;
        }

        public void SendWebhookMessage ( string message )
        {
            try
            {
                WebClient client = new WebClient ();
                var body = JsonConvert.SerializeObject ( new { username = _applicationSettings.DiscordUserName, embeds = new List<object> { new { description = message } } } );


            } catch (Exception ex )
            {

            }
        }
    }
}
