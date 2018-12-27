using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MinecraftWrapper.Data;
using Newtonsoft.Json;

namespace MinecraftWrapper.Services
{
    public class WhiteListService
    {
        private readonly ILogger<WhiteListService> _logger;
        private readonly ApplicationSettings _applicationSettings;

        public WhiteListService( ILogger<WhiteListService> logger, IOptions<ApplicationSettings> options )
        {
            _logger = logger;
            _applicationSettings = options.Value;
        }

        public List<WhiteListEntry> GetWhiteListEntries ()
        {
            List<WhiteListEntry> entries = null;

            try
            {
                var jsonString = File.ReadAllText ( _applicationSettings.WhiteListPath );
                entries = JsonConvert.DeserializeObject<List<WhiteListEntry>> ( jsonString );
                
            } catch (Exception ex )
            {
                _logger.LogError ( ex, "An unexpected error occured while fetching whitelist" );
            }

            return entries;
        }

        //public void SaveWhitelist (IEnumerable<WhiteListEntry> entries)
        //{
        //    entries = entries.Where ( e => !e.DeletePending );

        //    var jsonString = JsonConvert.SerializeObject ( entries );
        //    File.WriteAllText ( _applicationSettings.WhiteListPath, jsonString );
        //}

        public void DeleteWhiteListEntry ( string name )
        {
            try
            {
                var entries = GetWhiteListEntries ();
                entries = entries.Where ( e => e.name != name ).ToList ();
                var jsonString = JsonConvert.SerializeObject ( entries );

                File.WriteAllText ( _applicationSettings.WhiteListPath, jsonString );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "An unexpected error occured while deleting from whitelist" );
            }
        }

        public void AddWhiteListEntry ( string name )
        {
            try
            {
                var entries = GetWhiteListEntries ();

                // Do not add the same one
                if ( entries.Any ( e => e.name == name ) )
                {
                    return;
                }

                entries.Add ( new WhiteListEntry { name = name, ignoresPlayerLimit = false } );
                var jsonString = JsonConvert.SerializeObject ( entries );

                File.WriteAllText ( _applicationSettings.WhiteListPath, jsonString );
            }
            catch ( Exception ex )
            {
                _logger.LogError ( ex, "An unexpected error occured while adding to whitelist" );
            }
        }
    }
}
