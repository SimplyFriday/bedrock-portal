using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MinecraftWrapper.Data;
using Newtonsoft.Json;

namespace MinecraftWrapper.Services
{
    public class DiscordService
    {
        private readonly ApplicationSettings _applicationSettings;
        private static HttpClient _client;

        public DiscordService ( IOptions<ApplicationSettings> options )
        {
            _applicationSettings = options.Value;
        }

        public async void SendWebhookMessage ( string message )
        {
            try
            {
                if ( !string.IsNullOrEmpty ( _applicationSettings.DiscordWebhookUrl ) )
                {
                    if ( _client == null )
                    {
                        _client = HttpClientFactory.Create ();
                    }

                    _client.DefaultRequestHeaders.Accept.Clear ();
                    _client.DefaultRequestHeaders.Accept.Add ( new MediaTypeWithQualityHeaderValue ( "application/json" ) );

                    var body = JsonConvert.SerializeObject ( new { username = _applicationSettings.DiscordUserName, embeds = new List<object> { new { description = message } } } );
                    var response = await _client.PostAsync ( _applicationSettings.DiscordWebhookUrl, new StringContent ( body, Encoding.UTF8, "application/json" ) );
                }
            }
            catch ( Exception ex )
            {
                // TODO add logging
            }
        }
    }
}
