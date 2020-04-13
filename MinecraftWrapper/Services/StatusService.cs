using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MinecraftWrapper.Data;
using MinecraftWrapper.Data.Entities;
using MinecraftWrapper.Models;

namespace MinecraftWrapper.Services
{
    public class StatusService
    {
        private Dictionary<string, bool> _onlineUsers = new Dictionary<string, bool>();
        private DateTime _refreshTime;

        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationSettings _applicationSettings;

        private const int MINUTES_TO_CACHE = 20;

        public StatusService ( IServiceProvider serviceProvider, IOptions<ApplicationSettings> options )
        {
            _serviceProvider = serviceProvider;
            _ = RefreshUserList ();
            _applicationSettings = options.Value;
        }

        public bool IsUserOnline (string gametag )
        {
            return _onlineUsers.Any ( ou => ou.Key == gametag && ou.Value );
        }

        private async Task RefreshUserList ()
        {
            using ( var scope = _serviceProvider.CreateScope () )
            {
                var userRepository = scope.ServiceProvider.GetService<UserRepository> ();
                var allUsers = await userRepository.GetAllUsersAsync ();

                foreach ( var user in allUsers )
                {
                    if ( user.GamerTag != null && !_onlineUsers.ContainsKey ( user.GamerTag ) 
                        && (user.MembershipExpirationTime >= DateTime.UtcNow || !_applicationSettings.MembershipEnabled ) )
                    {
                        _onlineUsers.Add ( user.GamerTag, false );
                    }
                }

                _refreshTime = DateTime.UtcNow;
            }
        }

        public async Task<bool> GetUserStatus ( string userId )
        {
            using ( var scope = _serviceProvider.CreateScope () )
            {
                var user = await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>> ()
                    .Users
                    .SingleOrDefaultAsync (u => u.Id == userId);

                var gamerTag = user?.GamerTag;

                if ( gamerTag != null && _onlineUsers.ContainsKey ( gamerTag ) )
                {
                    return _onlineUsers [ gamerTag ];
                }
            }

            return false;
        }

        public void UpdateUserStatus ( string gamerTag, bool isOnline )
        {
            _ = RefreshUserList ();

            if ( gamerTag != null && _onlineUsers.ContainsKey ( gamerTag ) )
            {
                _onlineUsers [ gamerTag ] = isOnline;
            }

        }
    }
}
