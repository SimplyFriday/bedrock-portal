using Microsoft.Extensions.DependencyInjection;
using MinecraftWrapper.Data;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftWrapper.Services
{
    public partial class ScheduledTaskService
    {
        private async Task UpdateWhiteList ()
        {
            Log.Debug ( "Attempting to update whitelist." );

            using ( var scope = _serviceProvider.CreateScope () )
            {
                var entries = _whiteListService.GetWhiteListEntries();
                var users = await scope.ServiceProvider.GetRequiredService<UserRepository>().GetAllUsersAsync ();

                Log.Debug ( $"Found {users.Count} users. Starting iteration." );

                foreach ( var user in users )
                {
                    Log.Debug ( $"Checking {user.GamerTag} for membership" );
                    try
                    {
                        bool ignoreMembership = false;

                        if ( !_applicationSettings.MembershipEnabled || !_applicationSettings.StoreEnabled )
                        {
                            ignoreMembership = true;
                        }

                        if ( ( user.MembershipExpirationTime > DateTime.UtcNow || ignoreMembership ) && !entries.Any ( e => e.name == user.GamerTag ) && user.IsActive )
                        {
                            Log.Information ( $"Adding {user.GamerTag} to the whitelist." );
                            _whiteListService.AddWhiteListEntry ( user.GamerTag );
                        }

                        if ( ( ( ( user.MembershipExpirationTime == null || user.MembershipExpirationTime < DateTime.UtcNow ) && !ignoreMembership )
                            || !user.IsActive )
                            && entries.Any ( e => e.name == user.GamerTag ) )
                        {
                            Log.Information ( $"Removing {user.GamerTag} from the whitelist." );
                            _whiteListService.DeleteWhiteListEntry ( user.GamerTag );
                        }
                    }
                    catch ( Exception ex )
                    {
                        Log.Error ( ex, $"An exception occurred while trying to update the whitelist for user {user.GamerTag}" );
                    }
                }
            }
        }
    }
}
