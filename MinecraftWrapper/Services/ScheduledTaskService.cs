using Microsoft.Extensions.DependencyInjection;
using MinecraftWrapper.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MinecraftWrapper.Services
{
    public class ScheduledTaskService
    {
        public Dictionary<string,Delegate> RapidTasks = new Dictionary<string, Delegate> ();
        public Dictionary<string,Delegate> Tasks = new Dictionary<string, Delegate>  ();
        public Dictionary<string,Delegate> SlowTasks = new Dictionary<string, Delegate>  ();

        private bool _stopRequested = false;

        private readonly WhiteListService _whiteListService;
        private readonly IServiceProvider _serviceProvider;
        public ScheduledTaskService (WhiteListService whiteListService, IServiceProvider serviceProvider )
        {
            _whiteListService = whiteListService;
            _serviceProvider = serviceProvider;
        }

        public void Start ()
        {
            _ = RunRapidTasks ();
            _ = RunTasks ();
            _ = RunSlowTasks ();
        }

        private async Task RunRapidTasks ()
        {
            do
            {
                foreach (var t in RapidTasks )
                {
                    t.Value.DynamicInvoke ();
                }   

                await Task.Delay ( 10000 );
            } while ( !_stopRequested );
        }

        private async Task RunTasks ()
        {
            do
            {
                foreach ( var t in Tasks )
                {
                    t.Value.DynamicInvoke ();
                }

                await Task.Delay ( 60000 );
            } while ( !_stopRequested );
        }

        private async Task RunSlowTasks ()
        {
            do
            {
                foreach ( var t in SlowTasks )
                {
                    t.Value.DynamicInvoke ();
                }

                await Task.Delay ( 120000 );
            } while ( !_stopRequested );
        }

        public void Stop()
        {
            _stopRequested = true;
        }

        public void RegisterTasks ()
        {
            Tasks.Add ( "UpdateWhiteList", new Action( async () => await UpdateWhiteList() ) );
        }

        private async Task UpdateWhiteList ()
        {
            using ( var scope = _serviceProvider.CreateScope () )
            {
                var entries = _whiteListService.GetWhiteListEntries();
                var users = await scope.ServiceProvider.GetRequiredService<UserRepository>().GetAllUsersAsync ();

                foreach ( var user in users )
                {
                    if ( user.MembershipExpirationTime > DateTime.UtcNow && !entries.Any ( e => e.name == user.GamerTag ) && user.IsActive)
                    {
                        _whiteListService.AddWhiteListEntry ( user.GamerTag );
                    }

                    if ( ( user.MembershipExpirationTime < DateTime.UtcNow || !user.IsActive ) && entries.Any ( e => e.name == user.GamerTag ) ) 
                    {
                        _whiteListService.DeleteWhiteListEntry ( user.GamerTag );
                    }
                }
            }
        }
    }
}
