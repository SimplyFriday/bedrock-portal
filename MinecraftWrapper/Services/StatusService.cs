using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MinecraftWrapper.Data;
using MinecraftWrapper.Models;

namespace MinecraftWrapper.Services
{
    public class StatusService
    {
        private Dictionary<string, bool> _onlineUsers = new Dictionary<string, bool>();
        private DateTime _refreshTime;

        private readonly IServiceProvider _serviceProvider;

        private const int MINUTES_TO_CACHE = 20;

        public StatusService (IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            RefreshUserList ();
        }

        private void RefreshUserList ()
        {
            using ( var scope = _serviceProvider.CreateScope () )
            {
                var userRepository = scope.ServiceProvider.GetService<UserRepository> ();
                var allUsers = userRepository.GetUsersWithData ();

                foreach (var user in allUsers )
                {
                    if ( user.AdditionalUserData?.MinecraftId != null && !_onlineUsers.ContainsKey ( user.AdditionalUserData.MinecraftId ) )
                    {
                        _onlineUsers.Add ( user.AdditionalUserData.MinecraftId, false );
                    }
                }

                _refreshTime = DateTime.UtcNow;
            }
        }

        public bool GetUserStatus ( string userId )
        {
            if ( _refreshTime.AddMinutes ( MINUTES_TO_CACHE ) > DateTime.UtcNow )
            {
                RefreshUserList ();
            }

            using ( var scope = _serviceProvider.CreateScope () )
            {
                var user = scope.ServiceProvider.GetService<UserRepository> ()
                    .GetUsersWithData ()
                    .Where ( u => u.Id == userId )
                    .SingleOrDefault ();

                var id = user?.AdditionalUserData?.MinecraftId;

                if ( id != null && _onlineUsers.ContainsKey ( id ) )
                {
                    return _onlineUsers[ id ];
                }
            }

            return false;
        }

        public void UpdateUserStatus (string minecraftId, bool isOnline)
        {
            RefreshUserList ();

            using ( var scope = _serviceProvider.CreateScope () )
            {
                var id = scope.ServiceProvider.GetService<UserRepository> ()
                    .GetUsersWithData ()
                    .Where ( u => u.AdditionalUserData.MinecraftId == minecraftId )
                    .SingleOrDefault ()?.AdditionalUserData?.MinecraftId;

                if ( id != null && _onlineUsers.ContainsKey ( id ) )
                {
                    _onlineUsers[ minecraftId ] = isOnline;
                }
            }
        }
    }
}
